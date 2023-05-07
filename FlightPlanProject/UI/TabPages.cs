﻿using FlightPlan.KTools.UI;
using FPUtilities;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace FlightPlan;

public class BasePageContent : IPageContent
{
    public BasePageContent()
    {
        this.main_ui = FlightPlanUI.Instance;
        this.plugin = FlightPlanPlugin.Instance;
    }
    protected FlightPlanUI main_ui;
    protected FlightPlanPlugin plugin;


    protected PatchedConicsOrbit orbit => main_ui.Orbit;
    protected CelestialBodyComponent referenceBody => main_ui.ReferenceBody;

    public virtual string Name => throw new NotImplementedException();

    public virtual GUIContent Icon => throw new NotImplementedException();

    public bool IsRunning => false;


    bool ui_visible;
    public bool UIVisible { get => ui_visible; set => ui_visible = value; }

    public virtual bool IsActive => throw new NotImplementedException();

    public virtual void OnGUI()
    {
        throw new NotImplementedException();
    }
}

public class OwnshipManeuversPage : BasePageContent
{
    public override string Name => "Own Orbit";

    // readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/OwnshipManeuver_50v2.png");
    readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/Capsule_v3_50.png");

    public override GUIContent Icon => new(_tabIcon, "Ownship Maneuvers");

    public override bool IsActive => true;

    public override void OnGUI()
    {
        FPStyles.DrawSectionHeader("Ownship Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();
        main_ui.DrawToggleButton("Circularize", ManeuverType.circularize);
        // GUILayout.EndHorizontal();

        FPSettings.PeAltitude_km = main_ui.DrawToggleButtonWithTextField("New Pe", ManeuverType.newPe, FPSettings.PeAltitude_km, "km");
        main_ui.TargetPeR = FPSettings.PeAltitude_km * 1000 + referenceBody.radius;

        if (main_ui.Orbit.eccentricity < 1)
        {
            FPSettings.ApAltitude_km = main_ui.DrawToggleButtonWithTextField("New Ap", ManeuverType.newAp, FPSettings.ApAltitude_km, "km");
            main_ui.TargetApR = FPSettings.ApAltitude_km * 1000 + referenceBody.radius;
            main_ui.DrawToggleButton("New Pe & Ap", ManeuverType.newPeAp);
        }

        FPSettings.TargetInc_deg = main_ui.DrawToggleButtonWithTextField("New Inclination", ManeuverType.newInc, FPSettings.TargetInc_deg, "°");

        if (plugin._experimental.Value)
        {
            FPSettings.TargetLAN_deg = main_ui.DrawToggleButtonWithTextField("New LAN", ManeuverType.newLAN, FPSettings.TargetLAN_deg, "°");

            // FPSettings.TargetNodeLong_deg = DrawToggleButtonWithTextField("New Node Longitude", ref newNodeLon, FPSettings.TargetNodeLong_deg, "°");
        }

        FPSettings.TargetSMA_km = main_ui.DrawToggleButtonWithTextField("New SMA", ManeuverType.newSMA, FPSettings.TargetSMA_km, "km");
        main_ui.TargetSMA = FPSettings.TargetSMA_km * 1000 + referenceBody.radius;
    }
}

public class TargetPage : BasePageContent
{
    public override string Name => "Target";

    // readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50v2.png");
    readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/Capsule_x2v3_50.png");

    public override GUIContent Icon => new(_tabIcon, "Target Relative Maneuvers");

    public override bool IsActive
    {
        get => plugin._currentTarget != null  // If there is a target
            && plugin._currentTarget.Orbit != null // And the target is not a star
            && plugin._currentTarget.Orbit.referenceBody.Name == referenceBody.Name; // If the ActiveVessel and the _currentTarget are both orbiting the same body
    }

    public override void OnGUI()
    {
        FPStyles.DrawSectionHeader("Target Relative Maneuvers");

        BurnTimeOption.Instance.OptionSelectionGUI();

        main_ui.DrawToggleButton("Match Planes", ManeuverType.matchPlane);
        main_ui.DrawToggleButton("Hohmann Transfer", ManeuverType.hohmannXfer);
        main_ui.DrawToggleButton("Course Correction", ManeuverType.courseCorrection);

        if (plugin._experimental.Value)
        {
            FPSettings.InterceptTime = main_ui.DrawToggleButtonWithTextField("Intercept", ManeuverType.interceptTgt, FPSettings.InterceptTime, "s");
            main_ui.DrawToggleButton("Match Velocity", ManeuverType.matchVelocity);
        }
    }
}

public class InterplanetaryPage : BasePageContent
{
    public override string Name => "Target";

    // readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50v2.png");
    readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/Planet2_blue_50.png");

    public override GUIContent Icon => new(_tabIcon, "Orbital Transfer Maneuvers");

    public override bool IsActive
    {
        get => plugin._currentTarget != null // If the ActiveVessel is orbiting a planet and the current target is not the body the active vessel is orbiting
            && plugin._experimental.Value // No maneuvers relative to a star
            && !referenceBody.IsStar && plugin._currentTarget.IsCelestialBody
            && referenceBody.Orbit.referenceBody.IsStar && (plugin._currentTarget.Name != referenceBody.Name)
            && plugin._currentTarget.Orbit != null
            && plugin._currentTarget.Orbit.referenceBody.IsStar; // exclude targets that are a moon
    }
    public override void OnGUI()
    {
        FPStyles.DrawSectionHeader("Orbital Transfer Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();
        main_ui.DrawToggleButton("Interplanetary Transfer", ManeuverType.planetaryXfer);
    }
}


public class MoonPage : BasePageContent
{
    public override string Name => "Moon";

    // readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/TargetRelManeuver_50v2.png");
    readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/Moon_white_50.png");

    public override GUIContent Icon => new(_tabIcon, "Orbital Transfer Maneuvers");

    public override bool IsActive
    {
        get => !referenceBody.IsStar // not orbiting a star
                && !referenceBody.Orbit.referenceBody.IsStar && orbit.eccentricity < 1;
        // If the activeVessle is at a moon (a celestial in Orbit around another celestial that's not also a star)
    }
    public override void OnGUI()
    {
        FPStyles.DrawSectionHeader("Orbital Transfer Maneuvers");

        var parentPlanet = referenceBody.Orbit.referenceBody;
        FPSettings.MoonReturnAltitude_km = main_ui.DrawToggleButtonWithTextField("Moon Return", ManeuverType.moonReturn, FPSettings.MoonReturnAltitude_km, "km");
        main_ui.TargetMRPeR = FPSettings.MoonReturnAltitude_km * 1000 + parentPlanet.radius;
    }
}

public class ResonantOrbitPage : BasePageContent
{
    // Specialized buttons just for this tab
    private bool _nSatUp, _nSatDown, _nOrbUp, _nOrbDown, _setTgtPe, _setTgtAp, _setTgtSync, _setTgtSemiSync, _setTgtMinLOS;

    // Data this class needs to share between it's methods
    private double _synchronousAlt;
    private double _semiSynchronousAlt;
    private double _minLOSAlt;
    private string _targetAltitude = "600";      // String planned altitide for deployed satellites (destiantion Orbit)
    private double _target_alt_km = 600;         // Double planned altitide for deployed satellites (destiantion Orbit)
    private double _satPeriod;                   // The period of the destination Orbit
    private double _xferPeriod;                  // The period of the resonant deploy Orbit (_xferPeriod = _resonance*_satPeriod)
    // private bool _dive_error = false;

    // Data other classes and methods will need (needed to handle fixAp and fixPe maneuvers)
    public static double Ap2 { get; set; } // The resonant deploy Orbit apoapsis
    public static double Pe2 { get; set;  } // The resonant deploy Orbit periapsis

    public override string Name => "Resonant Orbit";

    // readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/ResonantOrbit_50v2.png");
    readonly Texture2D _tabIcon = AssetManager.GetAsset<Texture2D>($"{FlightPlanPlugin.Instance.SpaceWarpMetadata.ModID}/images/Satellite2_50.png");

    public override GUIContent Icon => new(_tabIcon, "Resonant Orbit Maneuvers");

    public override bool IsActive => true;

    public override void OnGUI()
    {
        FPStyles.DrawSectionHeader("Resonant Orbit Maneuvers");
        BurnTimeOption.Instance.OptionSelectionGUI();

        // Data only needed inside this method
        double _synchronousPeriod = plugin._activeVessel.mainBody.rotationPeriod;
        double _semiSynchronousPeriod = plugin._activeVessel.mainBody.rotationPeriod / 2;
        _synchronousAlt = SMACalc(_synchronousPeriod);
        _semiSynchronousAlt = SMACalc(_semiSynchronousPeriod);
        int _n, _m;

        // Determine if synchronous or semi-synchronous orbits are possible for this body
        if (_synchronousAlt > plugin._activeVessel.mainBody.sphereOfInfluence)
        {
            _synchronousAlt = -1;
        }
        if (_semiSynchronousAlt > plugin._activeVessel.mainBody.sphereOfInfluence)
        {
            _semiSynchronousAlt = -1;
        }

        // Set the _resonance factors based on diving or not
        _m = FPSettings.NumSats * FPSettings.NumOrbits;
        if (FPSettings.DiveOrbit) // If we're going to dive under the target Orbit for the deployment Orbit
            _n = _m - 1;
        else // If not
            _n = _m + 1;
        double _resonance = (double)_n / _m;
        string _resonanceStr = String.Format("{0}/{1}", _n, _m);

        // Compute the minimum LOS altitude
        _minLOSAlt = MinLOSCalc(FPSettings.NumSats, plugin._activeVessel.mainBody.radius, plugin._activeVessel.mainBody.hasAtmosphere);

        main_ui.DrawEntry2Button("Payloads:", ref _nSatUp, "+", ref _nSatDown, "-", FPSettings.NumSats.ToString(), "", "/"); // was numSatellites
        main_ui.DrawEntry2Button("Deploy Orbits:", ref _nOrbUp, "+", ref _nOrbDown, "-", FPSettings.NumOrbits.ToString(), "", "/"); // was numOrbits
        main_ui.DrawEntry("Orbital Resonance", _resonanceStr, " ");

        main_ui.DrawEntryTextField("Target Altitude", ref _targetAltitude, "km"); // Tried" FPSettings.tgt_altitude_km 

        main_ui.DrawEntryButton("Apoapsis", ref _setTgtAp, "⦾", $"{FPUtility.MetersToDistanceString(plugin._activeVessel.Orbit.ApoapsisArl / 1000)}", "km");
        main_ui.DrawEntryButton("Periapsis", ref _setTgtPe, "⦾", $"{FPUtility.MetersToDistanceString(plugin._activeVessel.Orbit.PeriapsisArl / 1000)}", "km");

        _satPeriod = PeriodCalc(_target_alt_km * 1000 + plugin._activeVessel.mainBody.radius);

        if (_synchronousAlt > 0)
        {
            main_ui.DrawEntryButton("Synchronous Alt", ref _setTgtSync, "⦾", $"{FPUtility.MetersToDistanceString(_synchronousAlt / 1000)}", "km");
            main_ui.DrawEntryButton("Semi Synchronous Alt", ref _setTgtSemiSync, "⦾", $"{FPUtility.MetersToDistanceString(_semiSynchronousAlt / 1000)}", "km");
        }
        else if (_semiSynchronousAlt > 0)
        {
            main_ui.DrawEntry("Synchronous Alt", "Outside SOI", " ");
            main_ui.DrawEntryButton("Semi Synchronous Alt", ref _setTgtSemiSync, "⦾", $"{FPUtility.MetersToDistanceString(_semiSynchronousAlt / 1000)}", "km");
        }
        else
        {
            main_ui.DrawEntry("Synchronous Alt", "Outside SOI", " ");
            main_ui.DrawEntry("Semi Synchronous Alt", "Outside SOI", " ");
        }
        main_ui.DrawEntry("SOI Alt", $"{FPUtility.MetersToDistanceString(plugin._activeVessel.mainBody.sphereOfInfluence / 1000)}", "km");
        if (_minLOSAlt > 0)
        {
            main_ui.DrawEntryButton("Min LOS Orbit Alt", ref _setTgtMinLOS, "⦾", $"{FPUtility.MetersToDistanceString(_minLOSAlt / 1000)}", "km");
        }
        else
        {
            main_ui.DrawEntry("Min LOS Orbit Alt", "Undefined", "km");
        }
        FPSettings.Occlusion = main_ui.DrawSoloToggle("<b>Occlusion</b>", FPSettings.Occlusion);
        if (FPSettings.Occlusion)
        {
            FPSettings.OccModAtm = main_ui.DrawEntryTextField("Atm", FPSettings.OccModAtm, "", KBaseStyle.TextInputStyle);
            GUILayout.Space(-FPStyles.SpacingAfterEntry);
            FPSettings.OccModVac = main_ui.DrawEntryTextField("Vac", FPSettings.OccModVac, "", KBaseStyle.TextInputStyle);
            GUILayout.Space(-FPStyles.SpacingAfterEntry);
        }

        // period1 = PeriodCalc(_target_alt_km*1000 + ActiveVessel.mainBody.radius);
        _xferPeriod = _resonance * _satPeriod;
        double _SMA2 = SMACalc(_xferPeriod);
        double _sSMA = _target_alt_km * 1000 + plugin._activeVessel.mainBody.radius;
        double _divePe = 2.0 * _SMA2 - _sSMA;
        if (_divePe < plugin._activeVessel.mainBody.radius) // No diving in the shallow end of the pool!
        {
            FPSettings.DiveOrbit = false;
            FPSettings.DiveOrbit = main_ui.DrawSoloToggle("<b>Dive</b>", FPSettings.DiveOrbit, true);
        }
        else
            FPSettings.DiveOrbit = main_ui.DrawSoloToggle("<b>Dive</b>", FPSettings.DiveOrbit);

        if (FPSettings.DiveOrbit)
        {
            Ap2 = _sSMA; // Diveing transfer orbits release at Apoapsis
            Pe2 = _divePe;
        }
        else
        {
            Pe2 = _sSMA; // Non-diving transfer orbits release at Periapsis
            Ap2 = 2.0 * _SMA2 - (Pe2);
        }
        double _ce = (Ap2 - Pe2) / (Ap2 + Pe2);
        
        main_ui.DrawEntry("Period", $"{FPUtility.SecondsToTimeString(_xferPeriod)}", "s");
        main_ui.DrawEntry("Apoapsis", $"{FPUtility.MetersToDistanceString((Ap2 - plugin._activeVessel.mainBody.radius) / 1000)}", "km");
        main_ui.DrawEntry("Periapsis", $"{FPUtility.MetersToDistanceString((Pe2 - plugin._activeVessel.mainBody.radius) / 1000)}", "km");
        main_ui.DrawEntry("Eccentricity", _ce.ToString("N3"), " ");
        double dV = BurnCalc(_sSMA, _sSMA, 0, Ap2, _SMA2, _ce, plugin._activeVessel.mainBody.gravParameter);
        main_ui.DrawEntry("Injection Δv", dV.ToString("N3"), "m/s");

        double _errorPe = (Pe2 - plugin._activeVessel.Orbit.Periapsis) / 1000;
        double _errorAp = (Ap2 - plugin._activeVessel.Orbit.Apoapsis) / 1000;
        string _fixPeStr, _fixApStr;

        GUILayout.Space(-FPStyles.SpacingAfterSection);

        UI_Tools.Separator();

        if (_errorPe > 0)
            _fixPeStr = $"Raise Pe to {((Pe2 - plugin._activeVessel.mainBody.radius) / 1000):N2} km";
        else
            _fixPeStr = $"Lower Pe to {((Pe2 - plugin._activeVessel.mainBody.radius) / 1000):N2} km";
        if (_errorAp > 0)
            _fixApStr = $"Raise Ap to {((Ap2 - plugin._activeVessel.mainBody.radius) / 1000):N2} km";
        else
            _fixApStr = $"Lower Ap to {((Ap2 - plugin._activeVessel.mainBody.radius) / 1000):N2} km";
        if (plugin._activeVessel.Orbit.Apoapsis < Pe2)
        {
            main_ui.DrawToggleButtonWithLabel("Fix Ap", ManeuverType.fixAp, _fixApStr, "", 55);
        }
        else if (plugin._activeVessel.Orbit.Periapsis > Ap2)
        {
            main_ui.DrawToggleButtonWithLabel("Fix Pe", ManeuverType.fixPe, _fixPeStr, "", 55);
        }
        else
        {
            if (Pe2 > plugin._activeVessel.mainBody.radius)
                main_ui.DrawToggleButtonWithLabel("Fix Ap", ManeuverType.fixPe, _fixPeStr, "", 55);
            main_ui.DrawToggleButtonWithLabel("Fix Pe", ManeuverType.fixAp, _fixApStr, "", 55);
        }

        HandleButtons();
    }

    private double OccModCalc(bool hasAtmo) // Specific to Resonant Orbits
    {
        double _occMod;
        if (FPSettings.Occlusion)
        {
            if (hasAtmo)
            {
                _occMod = FPSettings.OccModAtm;
            }
            else
            {
                _occMod = FPSettings.OccModVac;
            }
        }
        else
        {
            _occMod = 1;
        }
        return _occMod;
    }

    private double MinLOSCalc(int numSat, double radius, bool hasAtmo) // Specific to Resonant Orbits
    {
        if (numSat > 2)
        {
            return (radius * OccModCalc(hasAtmo)) / (Math.Cos(0.5 * (2.0 * Math.PI / numSat))) - radius;
        }
        else
        {
            return -1;
        }
    }

    public double SMACalc(double period) // General Purpose: Compute SMA given orbital period - RELOCATE TO ?
    {
        double _SMA;
        _SMA = Math.Pow((period * Math.Sqrt(plugin._activeVessel.mainBody.gravParameter) / (2.0 * Math.PI)), (2.0 / 3.0));
        return _SMA;
    }

    public double PeriodCalc(double SMA) // General Purpose: Compute orbital period given SMA - RELOCATE TO ?
    {
        double _period;
        _period = (2.0 * Math.PI * Math.Pow(SMA, 1.5)) / Math.Sqrt(plugin._activeVessel.mainBody.gravParameter);
        return _period;
    }

    private double BurnCalc(double sAp, double sSMA, double se, double cAp, double cSMA, double ce, double bGM)
    {
        double sta = 0;
        double cta = 0;
        if (cAp == sAp) cta = 180;
        double sr = sSMA * (1 - Math.Pow(se, 2)) / (1 + (se * Math.Cos(sta)));
        double sdv = Math.Sqrt(bGM * ((2 / sr) - (1 / sSMA)));

        double cr = cSMA * (1 - Math.Pow(ce, 2)) / (1 + (ce * Math.Cos(cta)));
        double cdv = Math.Sqrt(bGM * ((2 / sr) - (1 / cSMA)));

        return Math.Round(100 * Math.Abs(sdv - cdv)) / 100;
    }

    private void HandleButtons()
    {
        if (_nSatDown || _nSatUp || _nOrbDown || _nOrbUp || _setTgtPe || _setTgtAp || _setTgtSync || _setTgtSemiSync || _setTgtMinLOS)
        {
            // burnParams = Vector3d.zero;
            if (_nSatDown && FPSettings.NumSats > 2)
            {
                FPSettings.NumSats--;
                // numSatellites = FPSettings.NumSats.ToString();
            }
            else if (_nSatUp)
            {
                FPSettings.NumSats++;
                // numSatellites = FPSettings.NumSats.ToString();
            }
            else if (_nOrbDown && FPSettings.NumOrbits > 1)
            {
                FPSettings.NumOrbits--;
                // numOrbits = FPSettings.num_orb.ToString();
            }
            else if (_nOrbUp)
            {
                FPSettings.NumOrbits++;
                // numOrbits = FPSettings.num_orb.ToString();
            }
            else if (_setTgtPe)
            {
                // Logger.LogInfo($"HandleButtons: Setting tgt_altitude_km to Periapsis {ActiveVessel.Orbit.PeriapsisArl / 1000.0} km");
                _target_alt_km = plugin._activeVessel.Orbit.PeriapsisArl / 1000.0;
                _targetAltitude = _target_alt_km.ToString("N3");
                // Logger.LogInfo($"HandleButtons: tgt_altitude_km set to {_targetAltitude} km");
            }
            else if (_setTgtAp)
            {
                // Logger.LogInfo($"HandleButtons: Setting tgt_altitude_km to Apoapsis {ActiveVessel.Orbit.ApoapsisArl / 1000.0} km");
                _target_alt_km = plugin._activeVessel.Orbit.ApoapsisArl / 1000.0;
                _targetAltitude = _target_alt_km.ToString("N3");
                // Logger.LogInfo($"HandleButtons: tgt_altitude_km set to {_targetAltitude} km");

            }
            else if (_setTgtSync)
            {
                // Logger.LogInfo($"HandleButtons: Setting tgt_altitude_km to _synchronousAlt {_synchronousAlt / 1000.0} km");
                _target_alt_km = _synchronousAlt / 1000.0;
                _targetAltitude = _target_alt_km.ToString("N3");
                // Logger.LogInfo($"HandleButtons: tgt_altitude_km set to {_targetAltitude} km");

            }
            else if (_setTgtSemiSync)
            {
                // Logger.LogInfo($"HandleButtons: Setting tgt_altitude_km to _semiSynchronousAlt {_semiSynchronousAlt / 1000.0} km");
                _target_alt_km = _semiSynchronousAlt / 1000.0;
                _targetAltitude = _target_alt_km.ToString("N3");
                // Logger.LogInfo($"HandleButtons: tgt_altitude_km set to {_targetAltitude} km");

            }
            else if (_setTgtMinLOS)
            {
                // Logger.LogInfo($"HandleButtons: Setting tgt_altitude_km to _minLOSAlt {_minLOSAlt / 1000.0} km");
                _target_alt_km = _minLOSAlt / 1000.0;
                _targetAltitude = _target_alt_km.ToString("N3");
                // Logger.LogInfo($"HandleButtons: tgt_altitude_km set to {_targetAltitude} km");

            }
        }
    }

}
