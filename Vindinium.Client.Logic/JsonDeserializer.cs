﻿using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
	public class JsonDeserializer : IJsonDeserializer
	{
		#region IJsonDeserializer Members

		public T Deserialize<T>(string json) where T : class
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using (var stream = new MemoryStream(byteArray))
			{
				var ser = new DataContractJsonSerializer(typeof (T));
				return ser.ReadObject(stream) as T;
			}
		}

		#endregion
	}
}