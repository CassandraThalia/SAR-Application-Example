using Newtonsoft.Json;
using SAR.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Repository
{
    internal class JSONParser
    {
		public static string FilePath { get; set; }

		//This function returns an ObservableCollection that contains ALL of the info from the JSON file
		public static async Task<ObservableCollection<JSONObject>> GetRows()
		{
			ObservableCollection<JSONObject> JSONData = new ObservableCollection<JSONObject>();

			//Access Local Storage folder for temporary file storage
			Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
			Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("testFile.json");

			//Use Newtonsoft to deserialize the JSON data
			JSONData = JsonConvert.DeserializeObject<ObservableCollection<JSONObject>>(File.ReadAllText(sampleFile.Path));

			//Return a list of all data
			return JSONData;
		}
	}
}
