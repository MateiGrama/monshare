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
        private static readonly string CATEGORIES_IDS = "4d4b7104d754a06370d81259," + //Arts & Entertainment
                                                        "4d4b7105d754a06373d81259," + //Event
                                                        "4d4b7105d754a06376d81259," + //Nightlife Spot
                                                        "4d4b7105d754a06377d81259," + //Outdoors & Recreation
                                                        "4d4b7105d754a06378d81259," + //shop&service
                                                        "4d4b7105d754a06379d81259," + //travel&transport
                                                        "4d4b7105d754a06377d81259";   //outdoors


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
                        newPlace.Id = place["id"].ToString();
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
