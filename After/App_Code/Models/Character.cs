using After.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace After.Models
{
    public class Character
    {
        public Character()
        {
            CoreEnergy = 100;
            CoreEnergyPeak = 100;
            CurrentEnergy = 100;
            CurrentCharge = 0;
            CurrentWillpower = 100;
        }
        public long CharacterID { get; set; }
        public string Name { get; set; }
        public string Color { get; set; } = "gray";
        public string PortraitUri { get; set; }
        public double CoreEnergyPeak { get; set; }
        private double coreEnergy;
        public double CoreEnergy
        {
            get
            {
                return coreEnergy;
            }
            set
            {
                coreEnergy = value;
                if (value > CoreEnergyPeak)
                {
                    CoreEnergyPeak = value;
                }
            }
        }
        public double MaxEnergyModifier { get; set; }
        public double MaxEnergy
        {
            get
            {
                return CoreEnergy + MaxEnergyModifier;
            }
        }
        public double CurrentEnergy { get; set; }
        public double EnergyPercent
        {
            get
            {
                return CurrentEnergy / MaxEnergy;
            }
        }
        public double MaxChargeModifier { get; set; }
        public double MaxCharge
        {
            get
            {
                return CoreEnergy + MaxChargeModifier;
            }
        }
        public double CurrentCharge { get; set; }
        public double ChargePercent
        {
            get
            {
                return CurrentCharge / MaxCharge;
            }
        }
        public double MaxWillpowerModifier { get; set; }
        public double MaxWillpower
        {
            get
            {
                return CoreEnergy + MaxWillpowerModifier;
            }
        }
        public double CurrentWillpower { get; set; }
        public double WillpowerPercent
        {
            get
            {
                return CurrentWillpower / MaxWillpower;
            }
        }
        public string PreviousXYZ { get; set; }
       
        public string CurrentXYZ { get; set; }
        public double ViewDistance { get; set; } = 2;

        public bool IsCharging { get; set; }
        public string Interactions { get; set; }

        public MovementStates MovementState { get; set; }

        public enum MovementStates
        {
            Ready,
            Teleporting,
            Moving,
            Traveling,
            Combat,
            Dialog
        }
        public string Flags { get; set; }
        public bool IsHostile()
        {
            return false;
        }
        public bool IsHostile(Character ToCharacter)
        {
            return false;
        }
        public Location GetCurrentLocation()
        {
            while (CurrentXYZ == null)
            {
                Thread.Sleep(500);
            }
            var location = World.Current.Locations.FirstOrDefault(l => l.LocationID == CurrentXYZ);
            if (location == null)
            {
                location = World.Current.CreateTempLocation(CurrentXYZ.Split(','));
            }
            return location;
        }
        public Location GetPreviousLocation(World Context)
        {
            return Context.Locations.FirstOrDefault(l => l.LocationID == PreviousXYZ);
        }

        public void Move(string[] ToXYZ)
        {
            dynamic request;
            var toLocation = World.Current.Locations.Find($"{ToXYZ[0]},{ToXYZ[1]},{ToXYZ[2]}");
            if (toLocation == null)
            {
                toLocation = World.Current.CreateTempLocation(ToXYZ);
                var area = toLocation.ConvertToArea();
                request = new
                {
                    Category = "Events",
                    Type = "AreaCreated",
                    Area = area
                };
                foreach (var player in toLocation.GetNearbyPlayers())
                {
                    player.Send(Json.Encode(request));
                }
            }
            
            // TODO: Check if blocked.
            MovementState = MovementStates.Moving;
            var soul = ConvertToSoul();
            var currentLocation = GetCurrentLocation();
            var distance = currentLocation.GetDistanceFrom(toLocation);
            var travelTime = distance * 1000;
            var nearbyPlayers = currentLocation.GetNearbyPlayers();
            foreach (var player in toLocation.GetNearbyPlayers())
            {
                if (!nearbyPlayers.Contains(player))
                {
                    nearbyPlayers.Add(player);
                }
            }
            currentLocation.CharacterLeaves(this);
            request = Json.Encode(new
            {
                Category = "Events",
                Type = "PlayerMove",
                Soul = soul,
                From = currentLocation.LocationID,
                To = toLocation.LocationID,
                TravelTime = travelTime
            });
            foreach (var player in nearbyPlayers)
            {
                player.Send(request);
            }
            Task.Run(() => {
                Thread.Sleep((int)(Math.Round(travelTime)));
                CurrentXYZ = toLocation.LocationID;
                toLocation.CharacterArrives(this);
                MovementState = MovementStates.Ready;
            });
        }
        public dynamic ConvertToSoul()
        {
            var location = CurrentXYZ.Split(',');
            return new
            {
                CharacterID = this.CharacterID,
                Name = this.Name,
                Color = this.Color,
                XCoord = location[0],
                YCoord = location[1],
                ZCoord = location[2],
                CurrentXYZ = this.CurrentXYZ
            };
        }
    }
}
