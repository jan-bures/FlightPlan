

using KSP.Sim.impl;

using FlightPlan.KTools.UI;
using SpaceWarp.API.Assets;

using UnityEngine;

namespace FlightPlan;

public class BasePageContent : PageContent
{
    public BasePageContent()
    {
        this.main_ui = FlightPlanUI.Instance;
        this.plugin = FlightPlanPlugin.Instance;
    }
    protected FlightPlanUI main_ui;
    protected FlightPlanPlugin plugin;


    protected PatchedConicsOrbit orbit => main_ui.orbit;
    protected CelestialBodyComponent referenceBody => main_ui.referenceBody;

    public virtual string Name => throw new NotImplementedException();

    public virtual GUIContent Icon => throw new NotImplementedException();

    public bool isRunning => false;


    bool ui_visible;
    public bool UIVisible { get => ui_visible; set => ui_visible = value; }

    public virtual bool isActive => throw new NotImplementedException();

    public virtual void onGUI()
    {
        throw new NotImplementedException();
    }
}

public class OwnshipManeuversPage : BasePageContent
{
    public override string Name => "Own Orbit";

    readonly Texture2D tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/OwnshipManeuver_50.png");

    public override GUIContent Icon => new(tabIcon, "Ownship Maneuvers");

    public override bool isActive => true;

    public override void onGUI()
    {
        FPStyles.DrawSectionHeader("Ownship Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();
        main_ui.DrawToggleButton("Circularize", ManeuverType.circularize);
        // GUILayout.EndHorizontal();

        FPSettings.pe_altitude_km = main_ui.DrawToggleButtonWithTextField("New Pe", ManeuverType.newPe, FPSettings.pe_altitude_km, "km");
        main_ui.targetPeR = FPSettings.pe_altitude_km * 1000 + referenceBody.radius;

        if (main_ui.orbit.eccentricity < 1)
        {
            FPSettings.ap_altitude_km = main_ui.DrawToggleButtonWithTextField("New Ap", ManeuverType.newAp, FPSettings.ap_altitude_km, "km");
            main_ui.targetApR = FPSettings.ap_altitude_km * 1000 + referenceBody.radius;
            main_ui.DrawToggleButton("New Pe & Ap", ManeuverType.newPeAp);
        }

        FPSettings.target_inc_deg = main_ui.DrawToggleButtonWithTextField("New Inclination", ManeuverType.newInc, FPSettings.target_inc_deg, "�");

        if (plugin.experimental.Value)
        {
            FPSettings.target_lan_deg = main_ui.DrawToggleButtonWithTextField("New LAN", ManeuverType.newLAN, FPSettings.target_lan_deg, "�");

            // FPSettings.target_node_long_deg = DrawToggleButtonWithTextField("New Node Longitude", ref newNodeLon, FPSettings.target_node_long_deg, "�");
        }

        FPSettings.target_sma_km = main_ui.DrawToggleButtonWithTextField("New SMA", ManeuverType.newSMA, FPSettings.target_sma_km, "km");
        main_ui.targetSMA = FPSettings.target_sma_km * 1000 + referenceBody.radius;
    }
}

public class TargetPage : BasePageContent
{
    public override string Name => "Target";

    readonly Texture2D tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50.png");

    public override GUIContent Icon => new(tabIcon, "Target Relative Maneuvers");

    public override bool isActive
    {
        get => plugin.currentTarget != null  // If the is a target
            && plugin.currentTarget.Orbit != null // And the target is not a star
            && plugin.currentTarget.Orbit.referenceBody.Name == referenceBody.Name; // If the activeVessel and the currentTarget are both orbiting the same body
    }

    public override void onGUI()
    {
        FPStyles.DrawSectionHeader("Target Relative Maneuvers");

        BurnTimeOption.Instance.OptionSelectionGUI();

        main_ui.DrawToggleButton("Match Planes", ManeuverType.matchPlane);
        main_ui.DrawToggleButton("Hohmann Transfer", ManeuverType.hohmannXfer);
        main_ui.DrawToggleButton("Course Correction", ManeuverType.courseCorrection);

        if (plugin.experimental.Value)
        {
            FPSettings.interceptT = main_ui.DrawToggleButtonWithTextField("Intercept", ManeuverType.interceptTgt, FPSettings.interceptT, "s");
            main_ui.DrawToggleButton("Match Velocity", ManeuverType.matchVelocity);
        }
    }
}

public class InterplanetaryPage : BasePageContent
{
    public override string Name => "Target";

    readonly Texture2D tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50.png");

    public override GUIContent Icon => new(tabIcon, "Orbital Transfer Maneuvers");

    public override bool isActive
    {
        get => plugin.currentTarget != null // If the activeVessel is orbiting a planet and the current target is not the body the active vessel is orbiting
            && plugin.experimental.Value // No maneuvers relative to a star
            && !referenceBody.IsStar && plugin.currentTarget.IsCelestialBody
            && referenceBody.Orbit.referenceBody.IsStar && (plugin.currentTarget.Name != referenceBody.Name)
            && plugin.currentTarget.Orbit != null
            && plugin.currentTarget.Orbit.referenceBody.IsStar; // exclude targets that are a moon
    }
    public override void onGUI()
    {
        FPStyles.DrawSectionHeader("Orbital Transfer Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();
        main_ui.DrawToggleButton("Interplanetary Transfer", ManeuverType.planetaryXfer);
    }
}


public class MoonPage : BasePageContent
{
    public override string Name => "Moon";

    readonly Texture2D tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50.png");

    public override GUIContent Icon => new(tabIcon, "Orbital Transfer Maneuvers");

    public override bool isActive
    {
        get => !referenceBody.IsStar // not orbiting a star
                && !referenceBody.Orbit.referenceBody.IsStar && orbit.eccentricity < 1;
        // If the activeVessle is at a moon (a celestial in orbit around another celestial that's not also a star)

    }
    public override void onGUI()
    {
        FPStyles.DrawSectionHeader("Orbital Transfer Maneuvers");

        var parentPlanet = referenceBody.Orbit.referenceBody;
        FPSettings.mr_altitude_km = main_ui.DrawToggleButtonWithTextField("Moon Return", ManeuverType.moonReturn, FPSettings.mr_altitude_km, "km");
        main_ui.targetMRPeR = FPSettings.mr_altitude_km * 1000 + parentPlanet.radius;
    }
}

public class ResonantOrbitPage : BasePageContent
{
    public override string Name => "Resonant Orbit";

    readonly Texture2D tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/ResonantOrbit_50.png");

    public override GUIContent Icon => new(tabIcon, "Resonant Orbit Maneuvers");

    public override bool isActive => true;

    public override void onGUI()
    {
        FPStyles.DrawSectionHeader("Resonant Orbit Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();
        main_ui.DrawToggleButton("Circularize", ManeuverType.circularize);
        // GUILayout.EndHorizontal();

        FPSettings.pe_altitude_km = main_ui.DrawToggleButtonWithTextField("New Pe", ManeuverType.newPe, FPSettings.pe_altitude_km, "km");
        main_ui.targetPeR = FPSettings.pe_altitude_km * 1000 + referenceBody.radius;

        if (main_ui.orbit.eccentricity < 1)
        {
            FPSettings.ap_altitude_km = main_ui.DrawToggleButtonWithTextField("New Ap", ManeuverType.newAp, FPSettings.ap_altitude_km, "km");
            main_ui.targetApR = FPSettings.ap_altitude_km * 1000 + referenceBody.radius;
            main_ui.DrawToggleButton("New Pe & Ap", ManeuverType.newPeAp);
        }

        FPSettings.target_inc_deg = main_ui.DrawToggleButtonWithTextField("New Inclination", ManeuverType.newInc, FPSettings.target_inc_deg, "�");

        if (plugin.experimental.Value)
        {
            FPSettings.target_lan_deg = main_ui.DrawToggleButtonWithTextField("New LAN", ManeuverType.newLAN, FPSettings.target_lan_deg, "�");

            // FPSettings.target_node_long_deg = DrawToggleButtonWithTextField("New Node Longitude", ref newNodeLon, FPSettings.target_node_long_deg, "�");
        }

        FPSettings.target_sma_km = main_ui.DrawToggleButtonWithTextField("New SMA", ManeuverType.newSMA, FPSettings.target_sma_km, "km");
        main_ui.targetSMA = FPSettings.target_sma_km * 1000 + referenceBody.radius;
    }
}

