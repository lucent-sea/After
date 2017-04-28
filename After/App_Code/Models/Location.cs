using After.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Timers;
using System.Web.Helpers;

namespace After.Models
{
    public class Location
    {
        public double XCoord { get; set; }
        public double YCoord { get; set; }
        public string ZCoord { get; set; }
        public string LocationID
        {
            get
            {
                return XCoord.ToString() + "," + YCoord.ToString() + "," + ZCoord.ToString();
            }
            set
            {
                var split = value.Split(',');
                XCoord = double.Parse(split[0]);
                YCoord = double.Parse(split[1]);
                ZCoord = split[2];
            }
        }
        public string Color { get; set; } = "darkgray";

        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsStatic { get; set; }
        public DateTime? LastVisited { get; set; }
        public string LastVisitedBy { get; set; }
        public long InvestedWillpower { get; set; }
        public List<Character> GetOccupants()
        {
            var characters = World.Current.Characters.Where(p => p.CurrentXYZ == this.LocationID).ToList();
            if (characters.Count > 0)
            {
                return characters;
            }
            else
            {
                return null;
            };
        }
        public bool IsInnerVoid { get; set; }
        public long OwnerID { get; set; }
        public string Interactions { get; set; }


        //*** Utility Methods ***//
        public bool Exists(string XYZ, Socket_Handler SH)
        {
            if (World.Current.Locations.FirstOrDefault(loc => loc.LocationID == XYZ) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ContainsOccupant(Character CharacterObject)
        {
            return GetOccupants().ToList().Exists(cha => cha.CharacterID == CharacterObject.CharacterID);
        }
        public double GetDistanceFrom(Location FromLocation)
        {
            if (FromLocation.ZCoord != ZCoord)
            {
                return double.MaxValue;
            }
            return Math.Sqrt(
                Math.Pow(FromLocation.XCoord - XCoord, 2) +
                Math.Pow(FromLocation.YCoord - YCoord, 2)
            );
        }
        public List<Location> GetNearbyLocations(Character CharacterObject)
        {
            var locations = World.Current.Locations.Where(l => l.ZCoord == this.ZCoord &&
                Math.Abs(l.XCoord - this.XCoord) <= CharacterObject.ViewDistance &&
                Math.Abs(l.YCoord - this.YCoord) <= CharacterObject.ViewDistance);
            return locations?.ToList();
        }
        public List<Socket_Handler> GetNearbyPlayers()
        {
            return Socket_Handler.SocketCollection.Cast<Socket_Handler>().Where(sock => sock.Player.GetCurrentLocation()?.GetDistanceFrom(this) <= sock.Player.ViewDistance).ToList();
        }
        public void CharacterArrives(Character CharacterObject)
        {
            CharacterObject.CurrentXYZ = LocationID;
            LastVisited = DateTime.Now;
            LastVisitedBy = CharacterObject.Name;
            var soul = CharacterObject.ConvertToSoul();
            var nearbyPlayers = GetNearbyPlayers();
            var request = Json.Encode(new
            {
                Category = "Events",
                Type = "CharacterArrives",
                Soul = soul
            });
            foreach (var player in nearbyPlayers)
            {
                player.Send(request);
            }
        }
        public void CharacterLeaves(Character CharacterObject)
        {
            var soul = CharacterObject.ConvertToSoul();
            var nearbyPlayers = GetNearbyPlayers();
            CharacterObject.CurrentXYZ = null;
            var request = Json.Encode(new
            {
                Category = "Events",
                Type = "CharacterLeaves",
                Soul = soul
            });
            foreach (var player in nearbyPlayers)
            {
                player.Send(request);
            }
        }
        public dynamic ConvertToArea()
        {
            return new
            {
                Type = "Area",
                XCoord = this.XCoord,
                YCoord = this.YCoord,
                ZCoord = this.ZCoord,
                LocationID = this.LocationID,
                Color = this.Color,
                Title = this.Title,
                Description = this.Description,
                InvestedWillpower = this.InvestedWillpower
            };
        }
    }
}
