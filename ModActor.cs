using System;
using System.Collections.Generic;
using NeoRune;
using UE.CoreUObject;
using UE.Dungeons;
using UE.Engine;
using UE.InputCore;
using UE.SlateCore;
using UE.SpicewoodGAS;
using UE.SWCoreGAS;
using UE.SWCoreGameplay;
using UE.UMG;

namespace SprintMomentum;

/// <summary>
/// Minecraft Dungeons II styled segmented stamina & momentum HUD widget:
/// dark outer border, slate-stone bevel frame, 8 beveled segments with highlight tops, and compact percentage readout.
/// </summary>
public class SprintHudWidget : ScreenWidget
{
    const int SegmentCount = 8;
    const float SegWidth = 15f;
    const float SegHeight = 9f;
    const float SegTopHeight = 2f;
    const float SegGap = 2f;
    const float MomBarWidth = SegmentCount * SegWidth + (SegmentCount - 1) * SegGap;

    List<UBorder> segHighlights = new List<UBorder>();
    List<UBorder> segBodies = new List<UBorder>();
    UTextBlock? pctLabel;
    USizeBox? momentumWrapBox;
    USizeBox? momentumFillBox;
    UBorder? momentumFill;
    UBorder? bevelFrame;

    public override void OnInitialized()
    {
        var tree = Ui.Tree(this);
        if (tree == null) return;

        var row = Ui.Row(tree);
        if (row == null) return;

        var leftCol = Ui.Column(tree);
        if (leftCol == null) return;

        var segRow = Ui.Row(tree);
        if (segRow != null)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                if (i > 0)
                {
                    var spacer = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
                    if (spacer != null)
                    {
                        spacer.SetWidthOverride(SegGap);
                        segRow.AddChildToHorizontalBox(spacer);
                    }
                }

                var segWrap = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
                var segCol = Ui.Column(tree);
                var topBox = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
                var topBorder = UGameplayStatics.SpawnObject(Unreal.ClassOf<UBorder>(), tree) as UBorder;
                var bodyBox = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
                var bodyBorder = UGameplayStatics.SpawnObject(Unreal.ClassOf<UBorder>(), tree) as UBorder;

                if (segWrap != null && segCol != null && topBox != null && topBorder != null && bodyBox != null && bodyBorder != null)
                {
                    segWrap.SetWidthOverride(SegWidth);
                    segWrap.SetHeightOverride(SegHeight);

                    topBox.SetWidthOverride(SegWidth);
                    topBox.SetHeightOverride(SegTopHeight);
                    topBorder.SetBrushColor(Ui.Color(0.45f, 0.98f, 0.62f, 1.0f));
                    topBox.AddChild(topBorder);
                    segCol.AddChildToVerticalBox(topBox);

                    bodyBox.SetWidthOverride(SegWidth);
                    bodyBox.SetHeightOverride(SegHeight - SegTopHeight);
                    bodyBorder.SetBrushColor(Ui.Color(0.22f, 0.84f, 0.42f, 1.0f));
                    bodyBox.AddChild(bodyBorder);
                    segCol.AddChildToVerticalBox(bodyBox);

                    segWrap.AddChild(segCol);
                    segRow.AddChildToHorizontalBox(segWrap);
                    segHighlights.Add(topBorder);
                    segBodies.Add(bodyBorder);
                }
            }
            leftCol.AddChildToVerticalBox(segRow);
        }

        momentumWrapBox = CreateMomentumBar(tree, MomBarWidth, 3f, out momentumFillBox, out momentumFill);
        if (momentumWrapBox != null)
        {
            var vGap = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
            if (vGap != null)
            {
                vGap.SetHeightOverride(3f);
                leftCol.AddChildToVerticalBox(vGap);
            }
            leftCol.AddChildToVerticalBox(momentumWrapBox);
        }

        var leftSlot = row.AddChildToHorizontalBox(leftCol);
        leftSlot?.SetVerticalAlignment(EVerticalAlignment.VAlign_Center);

        var textGap = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
        if (textGap != null)
        {
            textGap.SetWidthOverride(8f);
            row.AddChildToHorizontalBox(textGap);
        }

        pctLabel = Ui.Text(tree, "100%", 14, Ui.Color(0.95f, 0.96f, 0.98f, 1.0f));
        if (pctLabel != null)
        {
            pctLabel.SetShadowOffset(new FVector2D { X = 1.0, Y = 1.0 });
            pctLabel.SetShadowColorAndOpacity(Ui.Color(0.03f, 0.03f, 0.05f, 0.95f));
            var labelSlot = row.AddChildToHorizontalBox(pctLabel);
            labelSlot?.SetVerticalAlignment(EVerticalAlignment.VAlign_Center);
        }

        // Triple-layer Minecraft Dungeons II frame: deep slate fill -> stone bevel -> dark outline
        var innerPanel = Ui.Panel(tree, row, Ui.Color(0.08f, 0.10f, 0.135f, 0.92f), 5f);
        bevelFrame = Ui.Panel(tree, innerPanel, Ui.Color(0.24f, 0.28f, 0.35f, 0.95f), 1f);
        var outerOutline = Ui.Panel(tree, bevelFrame, Ui.Color(0.035f, 0.04f, 0.055f, 0.96f), 2f);
        if (outerOutline != null)
        {
            outerOutline.SetVisibility(ESlateVisibility.HitTestInvisible);
            tree.RootWidget = outerOutline;
        }
        SetVisibility(ESlateVisibility.HitTestInvisible);
    }

    static USizeBox? CreateMomentumBar(
        UWidgetTree tree,
        float width,
        float height,
        out USizeBox? fillBox,
        out UBorder? fillBorder)
    {
        fillBox = null;
        fillBorder = null;

        var outer = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
        var overlay = UGameplayStatics.SpawnObject(Unreal.ClassOf<UOverlay>(), tree) as UOverlay;
        var bg = UGameplayStatics.SpawnObject(Unreal.ClassOf<UBorder>(), tree) as UBorder;
        fillBox = UGameplayStatics.SpawnObject(Unreal.ClassOf<USizeBox>(), tree) as USizeBox;
        fillBorder = UGameplayStatics.SpawnObject(Unreal.ClassOf<UBorder>(), tree) as UBorder;
        if (outer == null || overlay == null || bg == null || fillBox == null || fillBorder == null) return null;

        outer.SetWidthOverride(width);
        outer.SetHeightOverride(height);
        outer.AddChild(overlay);

        bg.SetBrushColor(Ui.Color(0.04f, 0.05f, 0.07f, 0.95f));
        var bgSlot = overlay.AddChildToOverlay(bg);
        bgSlot?.SetHorizontalAlignment(EHorizontalAlignment.HAlign_Fill);
        bgSlot?.SetVerticalAlignment(EVerticalAlignment.VAlign_Fill);

        fillBorder.SetBrushColor(Ui.Color(0.24f, 0.78f, 0.98f, 1.0f));
        fillBox.SetWidthOverride(width);
        fillBox.SetHeightOverride(height);
        fillBox.AddChild(fillBorder);

        var fillSlot = overlay.AddChildToOverlay(fillBox);
        fillSlot?.SetHorizontalAlignment(EHorizontalAlignment.HAlign_Left);
        fillSlot?.SetVerticalAlignment(EVerticalAlignment.VAlign_Fill);

        return outer;
    }

    public void UpdateState(SprintState state, int sprintMode, bool ramReady, bool showStaminaBar, bool showMomentumBar)
    {
        FLinearColor bodyCol;
        FLinearColor topCol;

        if (state.Exhausted)
        {
            bodyCol = Ui.Color(0.84f, 0.20f, 0.22f, 1.0f);
            topCol = Ui.Color(1.0f, 0.45f, 0.45f, 1.0f);
        }
        else if (state.Stamina < 0.35)
        {
            bodyCol = Ui.Color(0.92f, 0.56f, 0.14f, 1.0f);
            topCol = Ui.Color(1.0f, 0.80f, 0.36f, 1.0f);
        }
        else if (ramReady)
        {
            bodyCol = Ui.Color(0.92f, 0.68f, 0.16f, 1.0f);
            topCol = Ui.Color(1.0f, 0.88f, 0.42f, 1.0f);
        }
        else if (state.IsSprinting)
        {
            bodyCol = Ui.Color(0.20f, 0.84f, 0.54f, 1.0f);
            topCol = Ui.Color(0.48f, 0.98f, 0.72f, 1.0f);
        }
        else
        {
            bodyCol = Ui.Color(0.20f, 0.74f, 0.40f, 1.0f);
            topCol = Ui.Color(0.42f, 0.90f, 0.58f, 1.0f);
        }

        FLinearColor partialBody = Ui.Color(bodyCol.R * 0.48f, bodyCol.G * 0.48f, bodyCol.B * 0.48f, 0.95f);
        FLinearColor partialTop = Ui.Color(topCol.R * 0.48f, topCol.G * 0.48f, topCol.B * 0.48f, 0.95f);
        FLinearColor emptyBody = Ui.Color(0.06f, 0.075f, 0.10f, 0.95f);
        FLinearColor emptyTop = Ui.Color(0.11f, 0.13f, 0.17f, 0.95f);

        double filledSegments = showStaminaBar ? state.Stamina * SegmentCount : SegmentCount;
        for (int i = 0; i < segBodies.Count; i++)
        {
            if (filledSegments >= i + 0.85)
            {
                segBodies[i].SetBrushColor(bodyCol);
                segHighlights[i].SetBrushColor(topCol);
            }
            else if (filledSegments > i + 0.15)
            {
                segBodies[i].SetBrushColor(partialBody);
                segHighlights[i].SetBrushColor(partialTop);
            }
            else
            {
                segBodies[i].SetBrushColor(emptyBody);
                segHighlights[i].SetBrushColor(emptyTop);
            }
        }

        if (bevelFrame != null)
        {
            bevelFrame.SetBrushColor(ramReady
                ? Ui.Color(0.92f, 0.64f, 0.16f, 1.0f)
                : Ui.Color(0.24f, 0.28f, 0.35f, 0.95f));
        }

        if (pctLabel != null)
        {
            int stamPct = (int)Math.Round(state.Stamina * 100.0);
            if (state.Exhausted)
            {
                pctLabel.SetText($"! {stamPct}%");
                pctLabel.SetColorAndOpacity(Ui.SlateColor(Ui.Color(1.0f, 0.38f, 0.38f, 1.0f)));
            }
            else if (ramReady)
            {
                pctLabel.SetText("RAM!");
                pctLabel.SetColorAndOpacity(Ui.SlateColor(Ui.Color(1.0f, 0.84f, 0.24f, 1.0f)));
            }
            else
            {
                pctLabel.SetText($"{stamPct}%");
                pctLabel.SetColorAndOpacity(Ui.SlateColor(state.IsSprinting
                    ? Ui.Color(0.55f, 0.98f, 0.78f, 1.0f)
                    : Ui.Color(0.92f, 0.94f, 0.97f, 1.0f)));
            }
        }

        if (momentumWrapBox != null)
        {
            momentumWrapBox.SetVisibility(showMomentumBar ? ESlateVisibility.HitTestInvisible : ESlateVisibility.Collapsed);
        }

        if (momentumFillBox != null && momentumFill != null && showMomentumBar)
        {
            if (state.Momentum <= 0.005)
            {
                momentumFillBox.SetVisibility(ESlateVisibility.Collapsed);
            }
            else
            {
                momentumFillBox.SetVisibility(ESlateVisibility.HitTestInvisible);
                momentumFillBox.SetWidthOverride((float)(MomBarWidth * SprintMath.Clamp(state.Momentum, 0.02, 1.0)));
                momentumFill.SetBrushColor(ramReady
                    ? Ui.Color(0.98f, 0.66f, 0.14f, 1.0f)
                    : Ui.Color(0.28f, 0.80f, 0.98f, 1.0f));
            }
        }
    }
}

[ModSetting.Heading("Sprint Momentum")]
[ModSetting.Toggle("enabled", "Enable Sprint", Default = true, Description = "Turns Sprint Momentum on or off.")]
[ModSetting.Keybind("sprint_key", "Sprint Key", Default = "LeftShift", Description = "Key or gamepad button used to sprint (Hold or Toggle modes).")]
[ModSetting.Select("activation_mode", "Activation Mode", "Hold to Sprint", "Toggle Sprint", "Auto-Sprint")]
[ModSetting.Select("sprint_mode", "Sprint Style", "Instant + Ram (Sharp Start)", "Classic SprintMod (Instant)", "Gradual Ramp + Ram")]
[ModSetting.Heading("Speed & Stamina")]
[ModSetting.Slider("speed_mult", "Sprint Speed Multiplier", Default = 1.65, Min = 1.15, Max = 2.50, Step = 0.05, Description = "Movement speed multiplier while sprinting (1.65 = +65%).")]
[ModSetting.Slider("ramp_seconds", "Ram Charge Time (sec)", Default = 1.4, Min = 0.5, Max = 5.0, Step = 0.25, Description = "Time to charge full ram momentum (also speed ramp time in Gradual mode).")]
[ModSetting.Slider("stamina_drain", "Stamina Drain / sec", Default = 0.16, Min = 0.0, Max = 0.50, Step = 0.02, Description = "Stamina spent per second while sprinting. Set to 0 for infinite sprint.")]
[ModSetting.Slider("stamina_regen", "Stamina Recovery / sec", Default = 0.28, Min = 0.05, Max = 0.80, Step = 0.02, Description = "Stamina recovered per second while not sprinting.")]
[ModSetting.Heading("Momentum Ram")]
[ModSetting.Toggle("ram_enabled", "Ram Enemies at High Momentum", Default = true, Description = "Knock back and damage hostile mobs when running into them at high momentum.")]
[ModSetting.Slider("ram_threshold", "Ram Threshold", Default = 0.55, Min = 0.25, Max = 1.00, Step = 0.05, Description = "Minimum momentum required to ram enemies.")]
[ModSetting.Slider("ram_damage", "Ram Base Damage", Default = 35, Min = 0, Max = 250, Step = 5, Description = "Damage dealt to enemies hit during a momentum ram.")]
[ModSetting.Slider("ram_force", "Ram Knockback Force", Default = 750, Min = 200, Max = 1600, Step = 50, Description = "Knockback impulse applied to rammed enemies.")]
[ModSetting.Heading("HUD")]
[ModSetting.Select("hud_mode", "Stamina HUD Visibility", "Always Visible", "Auto (When Sprinting)", "Hidden")]
[ModSetting.Select("hud_position", "HUD Position", "Above Hotbar", "Bottom Left", "Bottom Right", "Top Left", "Top Center")]
[ModSetting.Slider("hud_offset_x", "HUD Horizontal Offset", Default = 0, Min = -600, Max = 600, Step = 10, Description = "Shift the stamina bar left or right.")]
[ModSetting.Slider("hud_offset_y", "HUD Vertical Margin", Default = 230, Min = 16, Max = 500, Step = 5, Description = "Vertical margin from the selected screen edge.")]
public class ModActor : AActor, IModSettings
{
    bool enabled = true;
    FKey sprintKey = new FKey { KeyName = "LeftShift" };
    FKey sprintKeySecondary = new FKey { KeyName = "None" };
    int activationMode = 0; // 0 = Hold, 1 = Toggle, 2 = Auto
    int sprintMode = 0;     // 0 = Instant + Ram, 1 = Classic Instant, 2 = Gradual + Ram
    double speedMult = 1.65;
    double rampSeconds = 1.4;
    double staminaDrain = 0.16;
    double staminaRegen = 0.28;
    bool ramEnabled = true;
    double ramThreshold = 0.55;
    double ramDamage = 35;
    double ramForce = 750;
    int hudMode = 0;
    int hudPosition = 0;
    double hudOffsetX = 0;
    double hudOffsetY = 230;

    KeyListener? primaryListener;
    KeyListener? secondaryListener;
    SprintHudWidget? hudWidget;
    bool hudVisible;

    SprintState state;
    bool isMenuLevel;
    bool toggleActive;
    bool primaryHeld;
    bool secondaryHeld;

    float baselineWalkSpeed;
    float lastAppliedWalkSpeed;
    bool speedWasModified;
    bool loggedSpeedOnce;
    bool loggedRamOnce;
    FVector prevLoc;
    bool hasPrevLoc;
    FVector lastMoveDir;
    double ramGraceUntil;

    double nextRamScanTime;
    List<ABaseCharacter> recentMobs = new List<ABaseCharacter>();
    List<double> recentMobUntil = new List<double>();

    protected override void ReceiveBeginPlay()
    {
        state = SprintMath.InitialState();
        isMenuLevel = World.LevelName(this).StartsWith("Menu");
        Log.Write($"SprintMomentum started on level '{World.LevelName(this)}'.");

        if (isMenuLevel) return;

        ListenToKeys();
    }

    protected override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
    {
        primaryListener?.Stop();
        secondaryListener?.Stop();
        RestoreSpeed();
        HideHud();
    }

    public void OnSettingChanged(string id, string value)
    {
        if (id == "enabled") enabled = ModSettings.ToBool(value);
        else if (id == "activation_mode") activationMode = ModSettings.ToInt(value);
        else if (id == "sprint_mode") sprintMode = ModSettings.ToInt(value);
        else if (id == "speed_mult") speedMult = ModSettings.ToNumber(value);
        else if (id == "ramp_seconds") rampSeconds = ModSettings.ToNumber(value);
        else if (id == "stamina_drain") staminaDrain = ModSettings.ToNumber(value);
        else if (id == "stamina_regen") staminaRegen = ModSettings.ToNumber(value);
        else if (id == "ram_enabled") ramEnabled = ModSettings.ToBool(value);
        else if (id == "ram_threshold") ramThreshold = ModSettings.ToNumber(value);
        else if (id == "ram_damage") ramDamage = ModSettings.ToNumber(value);
        else if (id == "ram_force") ramForce = ModSettings.ToNumber(value);
        else if (id == "hud_mode") hudMode = ModSettings.ToInt(value);
        else if (id == "hud_position" || id == "hud_offset_x" || id == "hud_offset_y")
        {
            if (id == "hud_position") hudPosition = ModSettings.ToInt(value);
            else if (id == "hud_offset_x") hudOffsetX = ModSettings.ToNumber(value);
            else hudOffsetY = ModSettings.ToNumber(value);
            if (hudVisible)
            {
                HideHud();
                ShowHud();
            }
        }

        if (!enabled)
        {
            toggleActive = false;
            state = SprintMath.InitialState();
            RestoreSpeed();
            HideHud();
        }
    }

    public void OnKeybindChanged(string id, FKey key, FKey secondaryKey)
    {
        if (id != "sprint_key") return;
        sprintKey = key;
        sprintKeySecondary = secondaryKey;
        if (!isMenuLevel) ListenToKeys();
    }

    public void OnSettingsReset()
    {
        enabled = true;
        sprintKey = new FKey { KeyName = "LeftShift" };
        sprintKeySecondary = new FKey { KeyName = "None" };
        activationMode = 0;
        sprintMode = 0;
        speedMult = 1.65;
        rampSeconds = 1.4;
        staminaDrain = 0.16;
        staminaRegen = 0.28;
        ramEnabled = true;
        ramThreshold = 0.55;
        ramDamage = 35;
        ramForce = 750;
        hudMode = 0;
        hudPosition = 0;
        hudOffsetX = 0;
        hudOffsetY = 230;

        toggleActive = false;
        primaryHeld = false;
        secondaryHeld = false;
        state = SprintMath.InitialState();
        if (!isMenuLevel) ListenToKeys();
        if (hudVisible)
        {
            HideHud();
            ShowHud();
        }
    }

    void ListenToKeys()
    {
        primaryListener?.Stop();
        secondaryListener?.Stop();

        if (UKismetInputLibrary.Key_IsValid(sprintKey))
        {
            primaryListener = Input.OnKey(this, sprintKey);
            if (primaryListener != null)
            {
                primaryListener.Pressed += OnPrimaryPressed;
                primaryListener.Released += OnPrimaryReleased;
            }
        }

        if (UKismetInputLibrary.Key_IsValid(sprintKeySecondary))
        {
            secondaryListener = Input.OnKey(this, sprintKeySecondary);
            if (secondaryListener != null)
            {
                secondaryListener.Pressed += OnSecondaryPressed;
                secondaryListener.Released += OnSecondaryReleased;
            }
        }
    }

    void OnPrimaryPressed()
    {
        primaryHeld = true;
        if (activationMode == 1 && enabled) toggleActive = !toggleActive;
    }

    void OnPrimaryReleased() => primaryHeld = false;

    void OnSecondaryPressed()
    {
        secondaryHeld = true;
        if (activationMode == 1 && enabled) toggleActive = !toggleActive;
    }

    void OnSecondaryReleased() => secondaryHeld = false;

    public override void ReceiveTick(float DeltaSeconds)
    {
        if (isMenuLevel || !enabled) return;

        var player = World.Player(this) as APlayerCharacter;
        var controller = World.PlayerController(this);
        if (player == null || controller == null) return;

        var moveComp = player.CharacterMovement;
        if (moveComp == null) return;

        if (primaryHeld && (!UKismetInputLibrary.Key_IsValid(sprintKey) || !controller.IsInputKeyDown(sprintKey))) primaryHeld = false;
        if (secondaryHeld && (!UKismetInputLibrary.Key_IsValid(sprintKeySecondary) || !controller.IsInputKeyDown(sprintKeySecondary))) secondaryHeld = false;

        bool wantsSprint = activationMode == 2
            ? true
            : (activationMode == 1 ? toggleActive : (primaryHeld || secondaryHeld));

        var currLoc = player.K2_GetActorLocation();
        double posSpeed2D = 0.0;
        if (hasPrevLoc && DeltaSeconds > 0.0001f)
        {
            double dx = currLoc.X - prevLoc.X;
            double dy = currLoc.Y - prevLoc.Y;
            posSpeed2D = Math.Sqrt(dx * dx + dy * dy) / DeltaSeconds;
        }
        prevLoc = currLoc;

        var vel = player.GetVelocity();
        double speed2D = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
        if (speed2D > 50.0)
        {
            lastMoveDir = new FVector { X = (float)(vel.X / speed2D), Y = (float)(vel.Y / speed2D), Z = 0f };
        }
        else if (lastMoveDir.X == 0f && lastMoveDir.Y == 0f)
        {
            lastMoveDir = player.GetActorForwardVector();
        }

        var swMove = moveComp as USWMovementComponent;
        bool runningIntoWall = swMove != null && swMove.IsOwnerRunningIntoWall();
        bool isMoving = hasPrevLoc && SprintMath.IsActuallyMoving(speed2D, posSpeed2D, runningIntoWall);
        hasPrevLoc = true;

        state = SprintMath.Step(
            state,
            DeltaSeconds,
            wantsSprint,
            isMoving,
            sprintMode,
            speedMult,
            rampSeconds,
            staminaDrain,
            staminaRegen);

        ApplySpeed(moveComp);

        double now = World.RealTime(this);
        if (state.IsSprinting && state.Momentum >= ramThreshold && speed2D >= 150.0)
        {
            ramGraceUntil = now + 0.18;
        }

        bool ramReady = SprintMath.CanRamWithGrace(state, sprintMode, ramEnabled, ramThreshold, now <= ramGraceUntil);
        if (ramReady && now >= nextRamScanTime)
        {
            nextRamScanTime = now + 0.04;
            ProcessMomentumRam(player, now);
        }

        UpdateHud(ramReady);
    }

    void ApplySpeed(UCharacterMovementComponent moveComp)
    {
        float currentMax = moveComp.MaxWalkSpeed;

        if (!speedWasModified || Math.Abs(currentMax - lastAppliedWalkSpeed) > 1.5f)
        {
            if (currentMax > 10.0f) baselineWalkSpeed = currentMax;
        }

        if (baselineWalkSpeed <= 10.0f) return;

        if (state.SpeedMultiplier > 1.005)
        {
            float boosted = (float)(baselineWalkSpeed * state.SpeedMultiplier);
            moveComp.MaxWalkSpeed = boosted;
            lastAppliedWalkSpeed = boosted;
            speedWasModified = true;

            if (!loggedSpeedOnce)
            {
                loggedSpeedOnce = true;
                Log.Write($"Sprint active: baseSpeed={baselineWalkSpeed}, boosted={boosted}");
            }
        }
        else if (speedWasModified)
        {
            moveComp.MaxWalkSpeed = baselineWalkSpeed;
            lastAppliedWalkSpeed = baselineWalkSpeed;
            speedWasModified = false;
        }
    }

    void RestoreSpeed()
    {
        if (!speedWasModified || baselineWalkSpeed <= 10.0f) return;
        var player = World.Player(this) as APlayerCharacter;
        var moveComp = player?.CharacterMovement;
        if (moveComp != null) moveComp.MaxWalkSpeed = baselineWalkSpeed;
        speedWasModified = false;
    }

    void ProcessMomentumRam(APlayerCharacter player, double now)
    {
        for (int i = recentMobUntil.Count - 1; i >= 0; i--)
        {
            if (now >= recentMobUntil[i] || recentMobs[i] == null)
            {
                recentMobs.RemoveAt(i);
                recentMobUntil.RemoveAt(i);
            }
        }

        var playerLoc = player.K2_GetActorLocation();
        double dirX = lastMoveDir.X;
        double dirY = lastMoveDir.Y;

        var healthAttr = USWCoreGASUtil.GetAttributeChecked(Unreal.ClassOf<UATR_Health>(), "Health");
        var actors = World.FindAll(this, Unreal.ClassOf<ABaseCharacter>());
        bool hitAny = false;

        foreach (var actor in actors)
        {
            var target = actor as ABaseCharacter;
            if (target == null || target == player) continue;
            if (target.IsFriendlyTowards(player)) continue;
            if (!player.IsHostileTowards(target) && target is not AMobCharacter) continue;
            if (!target.IsTargetable()) continue;
            if (IsMobOnCooldown(target)) continue;

            var mobLoc = target.K2_GetActorLocation();
            double dx = mobLoc.X - playerLoc.X;
            double dy = mobLoc.Y - playerLoc.Y;
            double dz = Math.Abs(mobLoc.Z - playerLoc.Z);
            if (dz > 200.0) continue;

            double distSq = dx * dx + dy * dy;
            if (distSq > 185.0 * 185.0 || distSq < 1.0) continue;

            double dist = Math.Sqrt(distSq);
            double toMobX = dx / dist;
            double toMobY = dy / dist;

            // Point-blank contacts (< 95uu) always trigger; slightly further contacts require moving toward target.
            double dot = dirX * toMobX + dirY * toMobY;
            if (dist > 95.0 && dot < -0.35) continue;

            // 1. Knockback via PullCharacter (swept against walls) + LaunchCharacter
            float pullDist = (float)SprintMath.Clamp(ramForce * 0.14, 40.0, 180.0);
            var deltaPull = new FVector
            {
                X = (float)(toMobX * pullDist),
                Y = (float)(toMobY * pullDist),
                Z = 16f,
            };
            var hit = new FHitResult();
            target.PullCharacter(deltaPull, true, hit, ETeleportType.None);

            var knockVel = new FVector
            {
                X = (float)(toMobX * ramForce),
                Y = (float)(toMobY * ramForce),
                Z = 280f,
            };
            target.LaunchCharacter(knockVel, true, true);

            // 2. Apply real GAS Health damage via SWCoreGASUtil
            float prevHp = -1f;
            float newHp = -1f;
            if (ramDamage > 0.5 && target.AbilitySystemComponent != null)
            {
                if (USWCoreGASUtil.TryGetAttributeValueSlow(target.AbilitySystemComponent, "Health", out prevHp) && prevHp > 0f)
                {
                    newHp = prevHp - (float)ramDamage;
                    if (newHp <= 0f)
                    {
                        USWCoreGASUtil.SetNumericAttributeBaseToActor(target, healthAttr, 0f);
                        if (target.IsTargetable()) target.Kill();
                    }
                    else
                    {
                        USWCoreGASUtil.SetNumericAttributeBaseToActor(target, healthAttr, newHp);
                    }
                }
            }

            // 3. Visual hit flash on the struck character
            UDungeonsEffectLibrary.HitFlashOnCharacter(
                target,
                Ui.Color(1.0f, 0.90f, 0.65f, 1.0f),
                0.18f,
                0f,
                (float)UGameplayStatics.GetTimeSeconds(this));

            if (!loggedRamOnce)
            {
                loggedRamOnce = true;
                Log.Write($"Ram hit {UKismetSystemLibrary.GetObjectName(target)}: dist={dist:0}, hp={prevHp:0.0}->{newHp:0.0}");
            }

            recentMobs.Add(target);
            recentMobUntil.Add(now + 0.85);
            hitAny = true;
        }

        if (hitAny)
        {
            Sounds.Play(this, Sounds.ConfirmTag);
            state.Momentum = SprintMath.Clamp(state.Momentum - 0.15, 0.0, 1.0);
            ramGraceUntil = 0.0;
        }
    }

    bool IsMobOnCooldown(ABaseCharacter mob)
    {
        for (int i = 0; i < recentMobs.Count; i++)
        {
            if (recentMobs[i] == mob) return true;
        }
        return false;
    }

    void UpdateHud(bool ramReady)
    {
        bool shouldShow = SprintMath.ShouldShowHud(state, hudMode);
        if (!shouldShow)
        {
            if (hudVisible) HideHud();
            return;
        }

        if (!hudVisible) ShowHud();
        bool showMomBar = sprintMode != SprintMath.ModeClassicInstant && ramEnabled;
        hudWidget?.UpdateState(state, sprintMode, ramReady, staminaDrain > 0.0001, showMomBar);
    }

    void ShowHud()
    {
        if (hudWidget == null)
        {
            hudWidget = Ui.Create(this, Unreal.ClassOf<SprintHudWidget>()) as SprintHudWidget;
            if (hudWidget == null) return;
        }

        var anchor = new FVector2D { X = 0.5, Y = 1.0 };
        var offset = new FVector2D { X = hudOffsetX, Y = -hudOffsetY };
        if (hudPosition == 1) // Bottom Left
        {
            anchor = new FVector2D { X = 0.0, Y = 1.0 };
            offset = new FVector2D { X = 20.0 + hudOffsetX, Y = -hudOffsetY };
        }
        else if (hudPosition == 2) // Bottom Right
        {
            anchor = new FVector2D { X = 1.0, Y = 1.0 };
            offset = new FVector2D { X = -20.0 + hudOffsetX, Y = -hudOffsetY };
        }
        else if (hudPosition == 3) // Top Left
        {
            anchor = new FVector2D { X = 0.0, Y = 0.0 };
            offset = new FVector2D { X = 20.0 + hudOffsetX, Y = hudOffsetY };
        }
        else if (hudPosition == 4) // Top Center
        {
            anchor = new FVector2D { X = 0.5, Y = 0.0 };
            offset = new FVector2D { X = hudOffsetX, Y = hudOffsetY };
        }

        hudWidget.ShowAt(offset, anchor, ScreenWidget.AboveGameUI);
        hudVisible = true;
    }

    void HideHud()
    {
        hudWidget?.Hide();
        hudVisible = false;
    }
}
