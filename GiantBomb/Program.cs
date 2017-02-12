/*
 * Created by SharpDevelop.
 * User: Nick
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
	class Program
	{
		public static void Main(string[] args)
		{
			GiantBombPoller.getBoxArtByID("3030-4725");
			
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		
		

	}
	
	static class GiantBombPoller
	{
		// A private API key, obtained from GiantBomb.com to use their database
		private static string API_KEY = "eaaf363258bb7689b65331f7e0a4ae3dcbaf975c";
			
		public static bool getBoxArtByID(string gameID)
		{
			// Create a webClient to make requests to GiantBomb
			// Used: http://stackoverflow.com/questions/5566942/how-to-get-a-json-string-from-url
			//		 to debug user-agent problems			
			System.Net.WebClient wb = new System.Net.WebClient();
			wb.Headers.Add("User-Agent: VanderbiltUniversityCS4269");

			// Request a JSON string that contains image and name data for a given gameID.
			// Note: a valid API key is required
			string gameRequest = "http://www.giantbomb.com/api/game/" + gameID + 
				"/?api_key=" + API_KEY + "&format=json&field_list=game,image,name";
			try{
				string JSONResult = wb.DownloadString(gameRequest);
				
			// Deserialize the JSON so we can navigate it.
			// Used: http://stackoverflow.com/questions/3142495/deserialize-json-into-c-sharp-dynamic-object/9326146#9326146
			//		 for reference
			JavaScriptSerializer serializer =  new JavaScriptSerializer();
			dynamic item = serializer.Deserialize<object>(JSONResult);
			// Get the URL for the boxArt, as well as the gameTitle
			string boxArtURL = item["results"]["image"]["thumb_url"];
			string gameTitle = item["results"]["name"];
			
			// The filepath can not contain special characters, parse them out
			// TODO: Parse out additional special characters. Refactor into separate method
			string safeGameTitle = gameTitle.Replace(":", "");
			// Download the file as JPG
			// TODO: Create environment safe download path
			string download_path =  Path.Combine("D:\\Nick\\Pictures\\Game_BoxArt\\", safeGameTitle+".jpg");
			
			return downloadImage(boxArtURL, download_path);
			}
			catch(WebException wexc){
				Console.Write("Error requesting game data: " + gameID);
				return false;
			}
			
		}
		
		public static bool downloadImage(string imageUrl, string fileName)
		{
			try{
				System.Net.WebClient wb = new System.Net.WebClient();
				wb.Headers.Add("User-Agent: VanderbiltUniversityCS4269");
				wb.DownloadFile(imageUrl, fileName);
			}
			catch(WebException wexc){
				Console.WriteLine("Error downloading Image");
				return false;
			}
			return true;
		}

	}

}