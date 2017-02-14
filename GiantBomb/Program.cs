/*
 * Created by SharpDevelop.
 * User: Nick Lewis
 * Date: 2/8/2017
 * Time: 4:58 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;

namespace GiantBomb
{
	/* 
	 * Pulls NUM_SAMPLES cover art images from GiantBomb.com database of games
	 * Stores those images locally for labeling
	 */
	class RawDataFetcher
	{
		// The number of raw images to request
		private static int NUM_SAMPLES = 100;
		
		public static void Main(string[] args)
		{
			int successCount = 0;
			Random rnd = new Random();
			// Download image samples until we've reached NUM_SAMPLES images.
			while(successCount < NUM_SAMPLES){
				// Select a random gameID
				// GameIDs are between 1 and 58285 on GiantBomb.com (checked manually)
				int randomID = rnd.Next(1, 58285);
				Console.WriteLine("Checking gameID: "+ randomID);
				
				// Attempt to pull an image from GiantBomb, inc our successCount if success.
				if (GiantBombPoller.downloadBoxArtByID("3030-"+randomID))
				{
					successCount++;
				}
			}
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
	
	/*
	 * Given a gameID, use GiantBomb's API to request information
	 * about a game, including title, genre, and cover art.
	 */
	static class GiantBombPoller
	{
		// A private API key, obtained from GiantBomb.com to use their database
		private static string API_KEY = "eaaf363258bb7689b65331f7e0a4ae3dcbaf975c";
		
		/*
		 * Given a gameID, check if it contains the attributes we need
		 * (i.e. the ID is valid in the database, and it corresponds to
		 * at least 1 genre and 1 piece of cover art)
		 * Return if the gameID yielded data we need
		 */
		public static bool downloadBoxArtByID(string gameID)
		{
			// Create a webClient to make requests to GiantBomb
			// Used: http://stackoverflow.com/questions/5566942/how-to-get-a-json-string-from-url
			//		 to debug user-agent problems
			System.Net.WebClient wb = new System.Net.WebClient();
			wb.Headers.Add("User-Agent: VanderbiltUniversityCS4269");

			// Request a JSON string from GiantBomb that contains image, 
			// genre and name data for a given gameID.
			// Note: a valid API key is required
			string gameRequest = "http://www.giantbomb.com/api/game/" + gameID +
				"/?api_key=" + API_KEY + "&format=json&field_list=game,image,name,genres";
			try{
				string JSONResult = wb.DownloadString(gameRequest);
				
				// Deserialize the JSON so we can navigate it.
				// Used: http://stackoverflow.com/questions/3142495/deserialize-json-into-c-sharp-dynamic-object/9326146#9326146
				//		 for reference
				JavaScriptSerializer serializer =  new JavaScriptSerializer();
				dynamic item = serializer.Deserialize<object>(JSONResult);
				// If the database returns or error, no images exist, or no genres exist, return failure.
				if(item["error"] != "OK" || !item["results"].ContainsKey("image") || 
				   item["results"]["image"] == null || !item["results"].ContainsKey("genres"))
				{
					return false;
				}
				// Get the URL for the boxArt, as well as the gameTitle and genre
				string boxArtURL = item["results"]["image"]["super_url"];
				string gameGenre = item["results"]["genres"][0]["name"];
				string gameTitle = item["results"]["name"];
				
				// Pinball is a weird case where its cover art doesn't really follow
				// the other genres. Exclude Pinball samples.
				if (gameGenre == "Pinball")
					return false;
				// Write the information to a text file (ID, suggested genre) for further labeling.
				// TODO: Create environment safe write path
				File.AppendAllText("D:\\Nick\\Pictures\\Game_BoxArt\\Genres.txt", 
				                   gameID + " " + /*gameTitle + " " + */gameGenre + Environment.NewLine);
				
				// Download the file as JPG
				// TODO: Create environment safe download path
				string download_path =  Path.Combine("D:\\Nick\\Pictures\\Game_BoxArt\\", gameID+".jpg");
				
				// Try to download the image from the database, return the result.
				return downloadImage(boxArtURL, download_path);
			}
			// Catch failure polling GiantBomb servers
			catch(WebException wexc){
				Console.Write("Error requesting game data: " + gameID);
				return false;
			}
		}
		
		/*
		 * Given a url and a local filepath, download an image
		 * Return if download succeeded
		 */
		public static bool downloadImage(string imageUrl, string fileName)
		{
			try{
				// Create a new WebClient and attempt to download the image
				System.Net.WebClient wb = new System.Net.WebClient();
				wb.Headers.Add("User-Agent: VanderbiltUniversityCS4269");
				wb.DownloadFile(imageUrl, fileName);
			}
			// Catch failure downloading from GiantBomb
			catch(WebException wexc){
				Console.WriteLine("Error downloading Image");
				return false;
			}
			return true;
		}
	}
}