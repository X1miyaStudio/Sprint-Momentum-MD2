using NeoRune;

namespace SprintMomentum;

/// <summary>Pure simulation state for sprint momentum and stamina.</summary>
public struct SprintState
{
    public double Momentum;
    public double Stamina;
    public bool Exhausted;
    public bool IsSprinting;
    public double SpeedMultiplier;
}

/// <summary>Pure sprint math and decision logic (unit-tested outside the game).</summary>
public static class SprintMath
{
    public const int ModeInstantRam = 0;
    public const int ModeClassicInstant = 1;
    public const int ModeGradualRam = 2;

    public const int HudAlways = 0;
    public const int HudAuto = 1;
    public const int HudHidden = 2;

    public static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);

    public static SprintState InitialState() => new SprintState
    {
        Momentum = 0.0,
        Stamina = 1.0,
        Exhausted = false,
        IsSprinting = false,
        SpeedMultiplier = 1.0,
    };

    public static SprintState Step(
        SprintState state,
        double dt,
        bool wantsSprint,
        bool isMoving,
        int sprintMode,
        double maxSpeedMult,
        double rampSeconds,
        double drainPerSec,
        double regenPerSec)
    {
        dt = Clamp(dt, 0.0, 0.1);
        if (maxSpeedMult < 1.0) maxSpeedMult = 1.0;

        bool infiniteStamina = drainPerSec <= 0.0001;
        if (infiniteStamina)
        {
            state.Stamina = 1.0;
            state.Exhausted = false;
            state.IsSprinting = wantsSprint && isMoving;
        }
        else
        {
            if (wantsSprint && isMoving && !state.Exhausted && state.Stamina > 0.0)
            {
                state.IsSprinting = true;
                state.Stamina -= drainPerSec * dt;
                if (state.Stamina <= 0.0)
                {
                    state.Stamina = 0.0;
                    state.Exhausted = true;
                    state.IsSprinting = false;
                }
            }
            else
            {
                state.IsSprinting = false;
                double regenRate = isMoving ? regenPerSec * 0.65 : regenPerSec;
                state.Stamina = Clamp(state.Stamina + regenRate * dt, 0.0, 1.0);
                if (state.Exhausted && state.Stamina >= 0.30)
                {
                    state.Exhausted = false;
                }
            }
        }

        if (sprintMode == ModeClassicInstant)
        {
            state.Momentum = state.IsSprinting ? 1.0 : 0.0;
            state.SpeedMultiplier = state.IsSprinting ? maxSpeedMult : 1.0;
        }
        else
        {
            if (state.IsSprinting)
            {
                double safeRamp = rampSeconds < 0.1 ? 0.1 : rampSeconds;
                state.Momentum = Clamp(state.Momentum + dt / safeRamp, 0.0, 1.0);
            }
            else
            {
                state.Momentum = Clamp(state.Momentum - dt * 1.6, 0.0, 1.0);
            }

            if (sprintMode == ModeInstantRam)
            {
                // Sharp instant sprint speed immediately; momentum only charges the ram.
                state.SpeedMultiplier = state.IsSprinting ? maxSpeedMult : 1.0;
            }
            else
            {
                state.SpeedMultiplier = 1.0 + (maxSpeedMult - 1.0) * state.Momentum;
            }
        }

        return state;
    }

    public static bool IsActuallyMoving(double velocitySpeed2D, double positionSpeed2D, bool runningIntoWall)
    {
        if (runningIntoWall) return false;
        if (velocitySpeed2D < 100.0) return false;
        if (positionSpeed2D > 5000.0) return velocitySpeed2D >= 100.0;
        return positionSpeed2D >= 100.0;
    }

    public static bool CanRam(SprintState state, int sprintMode, bool ramEnabled, double ramThreshold, double speed2D) =>
        sprintMode != ModeClassicInstant && ramEnabled && state.IsSprinting && state.Momentum >= ramThreshold && speed2D >= 150.0;

    public static bool CanRamWithGrace(SprintState state, int sprintMode, bool ramEnabled, double ramThreshold, bool withinGrace) =>
        sprintMode != ModeClassicInstant && ramEnabled && state.Momentum >= ramThreshold && (state.IsSprinting || withinGrace);

    public static bool ShouldShowHud(SprintState state, int hudMode)
    {
        if (hudMode == HudHidden) return false;
        if (hudMode == HudAlways) return true;
        return state.IsSprinting || state.Momentum > 0.01 || state.Stamina < 0.99 || state.Exhausted;
    }

    public static string GetStatusText(SprintState state, int sprintMode, bool ramReady)
    {
        if (state.Exhausted) return "EXHAUSTED";
        if (ramReady && sprintMode != ModeClassicInstant) return "RAM READY!";
        if (state.IsSprinting) return "SPRINT";
        return "STAMINA";
    }
}

[ModTests]
public class SprintMathTests
{
    public void InstantRamModeGivesFullSpeedOnFirstFrameWhileChargingMomentum()
    {
        var s = SprintMath.InitialState();
        s = SprintMath.Step(s, 0.016, true, true, SprintMath.ModeInstantRam, 1.65, 2.0, 0.15, 0.30);
        ModAssert.True(s.IsSprinting);
        ModAssert.Near(1.65, s.SpeedMultiplier, 0.001);
        ModAssert.True(s.Momentum > 0.0 && s.Momentum < 0.05);

        s = SprintMath.Step(s, 0.016, false, true, SprintMath.ModeInstantRam, 1.65, 2.0, 0.15, 0.30);
        ModAssert.False(s.IsSprinting);
        ModAssert.Near(1.0, s.SpeedMultiplier, 0.001);
    }

    public void GradualModeRampsSpeedAndUnlocksRam()
    {
        var s = SprintMath.InitialState();
        for (int i = 0; i < 10; i++)
            s = SprintMath.Step(s, 0.1, true, true, SprintMath.ModeGradualRam, 1.6, 2.0, 0.0, 0.3);

        ModAssert.Near(0.5, s.Momentum, 0.001);
        ModAssert.Near(1.3, s.SpeedMultiplier, 0.001);

        for (int i = 0; i < 10; i++)
            s = SprintMath.Step(s, 0.1, true, true, SprintMath.ModeGradualRam, 1.6, 2.0, 0.0, 0.3);

        ModAssert.Near(1.0, s.Momentum, 0.001);
        ModAssert.Near(1.6, s.SpeedMultiplier, 0.001);
        ModAssert.True(SprintMath.CanRam(s, SprintMath.ModeGradualRam, true, 0.85, 300.0));
        ModAssert.False(SprintMath.CanRam(s, SprintMath.ModeClassicInstant, true, 0.85, 300.0));
    }

    public void StaminaExhaustionLocksSprintUntilThirtyPercentRecovery()
    {
        var s = SprintMath.InitialState();
        for (int i = 0; i < 17; i++)
            s = SprintMath.Step(s, 0.1, true, true, SprintMath.ModeInstantRam, 1.65, 2.0, 0.6, 0.2);

        ModAssert.True(s.Exhausted);
        ModAssert.False(s.IsSprinting);
        ModAssert.Near(0.0, s.Stamina, 0.001);

        // Recover slightly below 30% -> still exhausted
        s = SprintMath.Step(s, 0.1, true, false, SprintMath.ModeInstantRam, 1.65, 2.0, 0.6, 1.0);
        ModAssert.True(s.Exhausted);

        // Cross 30% -> recovered
        s = SprintMath.Step(s, 0.1, false, false, SprintMath.ModeInstantRam, 1.65, 2.0, 0.6, 2.5);
        ModAssert.False(s.Exhausted);
        ModAssert.True(s.Stamina >= 0.30);
    }

    public void HudVisibilityModesWorkCorrectly()
    {
        var idle = SprintMath.InitialState();
        ModAssert.True(SprintMath.ShouldShowHud(idle, SprintMath.HudAlways));
        ModAssert.False(SprintMath.ShouldShowHud(idle, SprintMath.HudAuto));
        ModAssert.False(SprintMath.ShouldShowHud(idle, SprintMath.HudHidden));

        idle.IsSprinting = true;
        ModAssert.True(SprintMath.ShouldShowHud(idle, SprintMath.HudAuto));
    }

    public void WallCollisionAndStandingStillAreNotCountedAsMoving()
    {
        ModAssert.False(SprintMath.IsActuallyMoving(0.0, 0.0, false));
        ModAssert.False(SprintMath.IsActuallyMoving(700.0, 8.0, false));
        ModAssert.False(SprintMath.IsActuallyMoving(700.0, 700.0, true));
        ModAssert.False(SprintMath.IsActuallyMoving(0.0, 12000.0, false));
        ModAssert.True(SprintMath.IsActuallyMoving(700.0, 695.0, false));
        ModAssert.True(SprintMath.CanRamWithGrace(new SprintState { Momentum = 0.6, IsSprinting = false }, SprintMath.ModeInstantRam, true, 0.55, true));
        ModAssert.False(SprintMath.CanRamWithGrace(new SprintState { Momentum = 0.6, IsSprinting = false }, SprintMath.ModeInstantRam, true, 0.55, false));
    }
}
