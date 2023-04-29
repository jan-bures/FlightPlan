﻿/*
 * This Software was obtained from the MechJeb2 project (https://github.com/MuMech/MechJeb2) on 3/25/23
 * and was further modified as needed for compatibility with KSP2 and/or for incorporation into the
 * FlightPlan project (https://github.com/schlosrat/FlightPlan)
 * 
 * This work is relaesed under the same license(s) inherited from the originating version.
 */

using UnityEngine;
using KSP.Sim.impl;

namespace MuMech
{
    public enum TimeReference
    {
        COMPUTED, X_FROM_NOW, APOAPSIS, PERIAPSIS, ALTITUDE, EQ_ASCENDING, EQ_DESCENDING,
        REL_ASCENDING, REL_DESCENDING, CLOSEST_APPROACH,
        EQ_HIGHEST_AD, EQ_NEAREST_AD, REL_HIGHEST_AD, REL_NEAREST_AD
    };

    public class TimeSelector
    {
        private readonly string[] timeRefNames;

        public double universalTime;

        private readonly TimeReference[] allowedTimeRef;
        // [Persistent(pass = (int)Pass.Global)]
        private int currentTimeRef;

        public TimeReference timeReference { get {return allowedTimeRef[currentTimeRef];}}

        // Input parameters
        // [Persistent(pass = (int)Pass.Global)]
        public EditableTime leadTime = 0;
        // [Persistent(pass = (int)Pass.Global)]
        public EditableDoubleMult circularizeAltitude = new EditableDoubleMult(150000, 1000);

        public TimeSelector(TimeReference[] allowedTimeRef)
        {
            this.allowedTimeRef = allowedTimeRef;
            universalTime = 0;
            timeRefNames = new string[allowedTimeRef.Length];
            for (int i = 0 ; i < allowedTimeRef.Length ; ++i)
            {
                switch (allowedTimeRef[i])
                {
                    case TimeReference.COMPUTED:          timeRefNames[i] = "at the optimum time";                   break;//at the optimum time
                    case TimeReference.APOAPSIS:          timeRefNames[i] = "at the next apoapsis";                  break;//"at the next apoapsis"
                    case TimeReference.CLOSEST_APPROACH:  timeRefNames[i] = "at closest approach to target";         break;//"at closest approach to target"
                    case TimeReference.EQ_ASCENDING:      timeRefNames[i] = "at the equatorial AN";                  break;//"at the equatorial AN"
                    case TimeReference.EQ_DESCENDING:     timeRefNames[i] = "at the equatorial DN";                  break;//"at the equatorial DN"
                    case TimeReference.PERIAPSIS:         timeRefNames[i] = "at the next periapsis";                 break;//"at the next periapsis"
                    case TimeReference.REL_ASCENDING:     timeRefNames[i] = "at the next AN with the target";        break;//"at the next AN with the target."
                    case TimeReference.REL_DESCENDING:    timeRefNames[i] = "at the next DN with the target";        break;//"at the next DN with the target."
                    case TimeReference.X_FROM_NOW:        timeRefNames[i] = "after a fixed time";                    break;//"after a fixed time"
                    case TimeReference.ALTITUDE:          timeRefNames[i] = "at an altitude";                        break;//"at an altitude"
                    case TimeReference.EQ_NEAREST_AD:     timeRefNames[i] = "at the nearest equatorial AN/DN";       break;//"at the nearest equatorial AN/DN"
                    case TimeReference.EQ_HIGHEST_AD:     timeRefNames[i] = "at the cheapest equatorial AN/DN";      break;//"at the cheapest equatorial AN/DN"
                    case TimeReference.REL_NEAREST_AD:    timeRefNames[i] = "at the nearest AN/DN with the target";  break;//"at the nearest AN/DN with the target"
                    case TimeReference.REL_HIGHEST_AD:    timeRefNames[i] = "at the cheapest AN/DN with the target"; break;//"at the cheapest AN/DN with the target"

                    //case TimeReference.COMPUTED:          timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect1");  break;//at the optimum time
                    //case TimeReference.APOAPSIS:          timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect2");  break;//"at the next apoapsis"
                    //case TimeReference.CLOSEST_APPROACH:  timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect3");  break;//"at closest approach to target"
                    //case TimeReference.EQ_ASCENDING:      timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect4");  break;//"at the equatorial AN"
                    //case TimeReference.EQ_DESCENDING:     timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect5");  break;//"at the equatorial DN"
                    //case TimeReference.PERIAPSIS:         timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect6");  break;//"at the next periapsis"
                    //case TimeReference.REL_ASCENDING:     timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect7");  break;//"at the next AN with the target."
                    //case TimeReference.REL_DESCENDING:    timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect8");  break;//"at the next DN with the target."
                    //case TimeReference.X_FROM_NOW:        timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect9");  break;//"after a fixed time"
                    //case TimeReference.ALTITUDE:          timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect10"); break;//"at an altitude"
                    //case TimeReference.EQ_NEAREST_AD:     timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect11"); break;//"at the nearest equatorial AN/DN"
                    //case TimeReference.EQ_HIGHEST_AD:     timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect12"); break;//"at the cheapest equatorial AN/DN"
                    //case TimeReference.REL_NEAREST_AD:    timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect13"); break;//"at the nearest AN/DN with the target"
                    //case TimeReference.REL_HIGHEST_AD:    timeRefNames[i] = LocalizedString.Format("#MechJeb_Maneu_TimeSelect14"); break;//"at the cheapest AN/DN with the target"

                    //case TimeReference.COMPUTED:          timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect1");  break;//at the optimum time
                    //case TimeReference.APOAPSIS:          timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect2");  break;//"at the next apoapsis"
                    //case TimeReference.CLOSEST_APPROACH:  timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect3");  break;//"at closest approach to target"
                    //case TimeReference.EQ_ASCENDING:      timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect4");  break;//"at the equatorial AN"
                    //case TimeReference.EQ_DESCENDING:     timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect5");  break;//"at the equatorial DN"
                    //case TimeReference.PERIAPSIS:         timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect6");  break;//"at the next periapsis"
                    //case TimeReference.REL_ASCENDING:     timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect7");  break;//"at the next AN with the target."
                    //case TimeReference.REL_DESCENDING:    timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect8");  break;//"at the next DN with the target."
                    //case TimeReference.X_FROM_NOW:        timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect9");  break;//"after a fixed time"
                    //case TimeReference.ALTITUDE:          timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect10"); break;//"at an altitude"
                    //case TimeReference.EQ_NEAREST_AD:     timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect11"); break;//"at the nearest equatorial AN/DN"
                    //case TimeReference.EQ_HIGHEST_AD:     timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect12"); break;//"at the cheapest equatorial AN/DN"
                    //case TimeReference.REL_NEAREST_AD:    timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect13"); break;//"at the nearest AN/DN with the target"
                    //case TimeReference.REL_HIGHEST_AD:    timeRefNames[i] = Localizer.Format("#MechJeb_Maneu_TimeSelect14"); break;//"at the cheapest AN/DN with the target"
                }
            }
        }

        public void DoChooseTimeGUI()
        {
            // GUILayout.Label(Localizer.Format("#MechJeb_Maneu_STB"));//Schedule the burn
            GUILayout.Label("Schedule the burn");//Schedule the burn
            GUILayout.BeginHorizontal();
            currentTimeRef = GuiUtils.ComboBox.Box(currentTimeRef, timeRefNames, this);
            switch (timeReference)
            {
                // No additional parameters required
                case TimeReference.COMPUTED:
                case TimeReference.APOAPSIS:
                case TimeReference.CLOSEST_APPROACH:
                case TimeReference.EQ_ASCENDING:
                case TimeReference.EQ_DESCENDING:
                case TimeReference.PERIAPSIS:
                case TimeReference.REL_ASCENDING:
                case TimeReference.REL_DESCENDING:
                case TimeReference.EQ_NEAREST_AD:
                case TimeReference.EQ_HIGHEST_AD:
                case TimeReference.REL_NEAREST_AD:
                case TimeReference.REL_HIGHEST_AD:
                    break;

                case TimeReference.X_FROM_NOW:
                    // GuiUtils.SimpleTextBox(Localizer.Format("#MechJeb_of"), leadTime);//"of"
                    GuiUtils.SimpleTextBox("of", leadTime);//"of"
                    break;

                case TimeReference.ALTITUDE:
                    // GuiUtils.SimpleTextBox(Localizer.Format("#MechJeb_of"), circularizeAltitude, "km");//"of"
                    GuiUtils.SimpleTextBox("of", circularizeAltitude);//"of"
                    break;
            }
            GUILayout.EndHorizontal();
        }

        public double ComputeManeuverTime(PatchedConicsOrbit o, double UT, PatchedConicsOrbit target) // was MechJebModuleTargetController target
        {
            switch (allowedTimeRef[currentTimeRef])
            {
                case TimeReference.X_FROM_NOW:
                    UT += leadTime.val;
                    break;

                case TimeReference.APOAPSIS:
                    if (o.eccentricity < 1)
                    {
                        UT = o.NextApoapsisTime(UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception1"));//"Warning: orbit is hyperbolic, so apoapsis doesn't exist."
                        throw new Exception("Warning: orbit is hyperbolic, so apoapsis doesn't exist.");//"Warning: orbit is hyperbolic, so apoapsis doesn't exist."
                    }
                    break;

                case TimeReference.PERIAPSIS:
                    UT = o.NextPeriapsisTime(UT);
                    break;

                case TimeReference.CLOSEST_APPROACH:
                    if (target != null) // was target.NormalTargetExists
                    {
                        UT = o.NextClosestApproachTime(target, UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception2"));//"Warning: no target selected."
                        throw new Exception("Warning: no target selected.");//"Warning: no target selected."
                    }
                    break;

                case TimeReference.ALTITUDE:
                    if (circularizeAltitude > o.PeriapsisArl && (circularizeAltitude < o.ApoapsisArl || o.eccentricity >= 1))
                    {
                        UT = o.NextTimeOfRadius(UT, o.referenceBody.radius + circularizeAltitude);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception3"));//"Warning: can't circularize at this altitude, since current orbit does not reach it."
                        throw new Exception("Warning: can't circularize at this altitude, since current orbit does not reach it.");//"Warning: can't circularize at this altitude, since current orbit does not reach it."
                    }
                    break;

                case TimeReference.EQ_ASCENDING:
                    if (o.AscendingNodeEquatorialExists())
                    {
                        UT = o.TimeOfAscendingNodeEquatorial(UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception4"));//"Warning: equatorial ascending node doesn't exist."
                        throw new Exception("Warning: equatorial ascending node doesn't exist.");//"Warning: equatorial ascending node doesn't exist."
                    }
                    break;

                case TimeReference.EQ_DESCENDING:
                    if (o.DescendingNodeEquatorialExists())
                    {
                        UT = o.TimeOfDescendingNodeEquatorial(UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception5"));//"Warning: equatorial descending node doesn't exist."
                        throw new Exception("Warning: equatorial descending node doesn't exist.");//"Warning: equatorial descending node doesn't exist."
                    }
                    break;

                case TimeReference.EQ_NEAREST_AD:
                    if(o.AscendingNodeEquatorialExists())
                    {
                        UT = o.DescendingNodeEquatorialExists()
                            ? System.Math.Min(o.TimeOfAscendingNodeEquatorial(UT), o.TimeOfDescendingNodeEquatorial(UT))
                            : o.TimeOfAscendingNodeEquatorial(UT);
                    }
                    else if(o.DescendingNodeEquatorialExists())
                    {
                        UT = o.TimeOfDescendingNodeEquatorial(UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception6"));//Warning: neither ascending nor descending node exists.
                        throw new Exception("Warning: neither ascending nor descending node exists.");//Warning: neither ascending nor descending node exists.
                    }
                    break;

                case TimeReference.EQ_HIGHEST_AD:
                    if(o.AscendingNodeEquatorialExists())
                    {
                        if(o.DescendingNodeEquatorialExists())
                        {
                            var anTime = o.TimeOfAscendingNodeEquatorial(UT);
                            var dnTime = o.TimeOfDescendingNodeEquatorial(UT);
                            UT = o.GetOrbitalVelocityAtUTZup(anTime).magnitude <= o.GetOrbitalVelocityAtUTZup(dnTime).magnitude
                                ? anTime
                                : dnTime;
                        }
                        else
                        {
                            UT = o.TimeOfAscendingNodeEquatorial(UT);
                        }
                    }
                    else if(o.DescendingNodeEquatorialExists())
                    {
                        UT = o.TimeOfDescendingNodeEquatorial(UT);
                    }
                    else
                    {
                        // throw new OperationException(Localizer.Format("#MechJeb_Maneu_Exception7"));//"Warning: neither ascending nor descending node exists."
                        throw new Exception("Warning: neither ascending nor descending node exists.");//"Warning: neither ascending nor descending node exists."
                    }
                    break;
            }

            universalTime = UT;
            return universalTime;
        }
    }
}