using monshare.Models;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace monshare.Utils
{
    class FoursquarePlaces
    {
        private static readonly string BASEURL = "https://api.foursquare.com/v2/venues";
        private static readonly string SEARCH_API = BASEURL + "/search";
        private static readonly string SUCCESS_STATUS_CODE = "200";
        private static readonly string ERROR_STATUS_CODE = "400";
        private static readonly string CLIENT_ID = "5PL0IOLMAGQUMPEBPMAVRW3WGAHM2EKLSXO1WKFNTYY4ZF2F";
        private static readonly string CLIENT_SECRET = "V4MJHSXJ5H1G1AJAGTEKLPKBHKE1SJFJQPHZ0F0SC4YGXQB4";
        private static readonly string AUTH_URL_PARAMS = "client_id=" + CLIENT_ID + "&client_secret=" + CLIENT_SECRET + "&v=20190604";

        //TODO limit those to more relevant subcategories: https://developer.foursquare.com/docs/resources/categories
        private static readonly string CATEGORIES_IDS = "4bf58dd8d48988d1e2931735," + //Art Gallery
                                                        "4bf58dd8d48988d1e4931735," + //Bowling Alley
                                                        "4bf58dd8d48988d18e941735," + //Comedy Club
                                                        "4deefb944765f83613cdba6e," + //Historic Site
                                                        "5642206c498e4bfca532186c," + //Memorial Site
                                                        "52e81612bcbc57f1066b79eb," + //Mini Golf
                                                        "4bf58dd8d48988d17f941735," + //Movie Theatre
                                                        "4bf58dd8d48988d181941735," + //Museums
                                                        "4bf58dd8d48988d1f2931735," + //Performing Arts Venue
                                                        "507c8c4091d498d9fc8c67a9," + //Public Art
                                                        "4bf58dd8d48988d182941735," + //Theme Park
                                                        "56aa371be4b08b9a8d573520," + //Tour Provider
                                                        "4bf58dd8d48988d193941735," + //Water Park
                                                        "4bf58dd8d48988d17b941735," + //Zoo
                                                        "4bf58dd8d48988d1df941735"; 


        //docs: https://developer.foursquare.com/docs/api/venues/search
        public static async Task<List<Place>> SearchPlacesAsync(string querry)
        {
            List<Place> places = new List<Place>();

            var status = await Utils.CheckPermissions(Permission.Location);

            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please grant location permissions", "Ok");
                return places;
            }

            var position = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(1));
                
            string url = SEARCH_API + "?" + AUTH_URL_PARAMS    + "&" +
                            "categoryId=" + CATEGORIES_IDS     + "&" +
                            "query="      + querry             + "&" +
                            "ll="         + position.Latitude  + "," + 
                                            position.Longitude + "&" +
                            "radius="     + 15000              + "&" +
                            "limit="      + 4;

            JsonValue result = await ServerCommunication.GetResponse(url);

            try
            {
                string response_status = result["meta"]["code"].ToString();
                if (response_status == SUCCESS_STATUS_CODE)
                {
                    foreach (JsonValue place in result["response"]["venues"])
                    {
                        Place newPlace = new Place();
                        newPlace.Id = place["id"];
                        newPlace.Name = place["name"].ToString();
                        newPlace.Location = new Location()
                        {
                            Address = place["location"].ContainsKey("address") ? place["location"]["address"].ToString().Trim('"') : "",
                            Lat = Double.Parse(place["location"]["lat"].ToString()),
                            Long = Double.Parse(place["location"]["lng"].ToString())
                        };
                        
                        places.Add(newPlace);
                    }
                }
                else if(response_status == ERROR_STATUS_CODE)
                {
                    await (new Page()).DisplayAlert("Oh, znap.", "Forsquare api call error; reason: " + result["meta"]["errorDetail"], "meh");
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Oh, znap.", "Forsquare api call error: "+ e.Message, "meh");
            }

            return places;
        }
    }
}
