/* 
 * Copyright 2011 Felix Rüttiger
 * 
 * This file is part of VirtualBoxService.
 *
 * VirtualBoxService is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VirtualBoxService is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VirtualBoxService.  If not, see <http://www.gnu.org/licenses/>.
 * 
 **/

namespace VirtualBoxService.WrapperExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.Serialization;
	using System.Text.RegularExpressions;
    using System.IO;

	[DataContract]
	class ParsedDescription
	{
		const string VIRTUAL_BOX_SERVICE_OPEN_TAG = "<!VirtualboxService--";
		const string VIRTUAL_BOX_SERVICE_CLOSING_TAG = "--/VirtualboxService>";


        public enum ShutdownTypeEnum {
            SaveState,
            ACPIShutdown,
            HardOff,
        }

        [DataContract]
        private class JSONRepresentation : ParsedDescription {
            [DataMember(Name = "Autostart")]
            internal string AutostartString {
                get { return null; }
                set {
                    bool autostart;
                    if (bool.TryParse(value, out autostart)) {
                        Autostart = autostart;
                    }
                    else {
                        Autostart = null; //indicate that this value could not be parsed without interrupting Deserialization
                    }
                }
            }

            [DataMember(Name = "ShutdownType")]
            internal string ShutdownTypeString {
                get { return null; }
                set {
                    ShutdownTypeEnum shutdownType;
                    if (Enum.TryParse<ShutdownTypeEnum>(value, out shutdownType)) {
                        ShutdownType = shutdownType;
                    }
                    else {
                        ShutdownType = null;
                    }
                }
            }

            [DataMember(Name = "ACPIShutdownTimeout", IsRequired = false)]
            internal string ACPIShutdownTimeoutString {
                get { return null; }
                set {
                    int acpiShutdownTimeout;
                    if (int.TryParse(value, out acpiShutdownTimeout)) {
                        ACPIShutdownTimeout = acpiShutdownTimeout;
                    }
                    else {
                        ACPIShutdownTimeout = null;
                    }
                }
            }
        }

        private class JSONHelper {
            /*
             * Taken from: http://pietschsoft.com/post/2008/02/NET-35-JSON-Serialization-using-the-DataContractJsonSerializer.aspx
             * 
             * Example:
             * /// Our Person object to Serialize/Deserialize to JSON
             * [DataContract]
             * public class Person
             * {
             *   public Person() { }
             *   public Person(string firstname, string lastname)
             *   {
             *     this.FirstName = firstname;
             *     this.LastName = lastname;
             *   }
             *
             *   [DataMember]
             *   public string FirstName { get; set; }
             *
             *   [DataMember]
             *   public string LastName { get; set; }
             * }
             */

            public static string Serialize<T>(T obj) {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, obj);
                string retVal = Encoding.Default.GetString(ms.ToArray());
                ms.Dispose();
                return retVal;
            }

            public static T Deserialize<T>(string json) {
                T obj = Activator.CreateInstance<T>();
                MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                ms.Close();
                ms.Dispose();
                return obj;
            }
        }


		public bool? Autostart
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public ShutdownTypeEnum? ShutdownType
		{
			get;
			private set;
		}

		public int? ACPIShutdownTimeout {
			get;
			private set;
		}



		public static ParsedDescription Parse(string rawDescription)
		{
			Regex virtualboxServiceTagRE = new Regex(VIRTUAL_BOX_SERVICE_OPEN_TAG + @"(?<json>[\w\s{}\[\]:,""]+)" + VIRTUAL_BOX_SERVICE_CLOSING_TAG);
			if (virtualboxServiceTagRE.IsMatch(rawDescription)) {
				//VirtualBoxService-Tag found --> Parse JSON and set Values
				Match virtualBoxServiceTag = virtualboxServiceTagRE.Match(rawDescription);
				string json = virtualBoxServiceTag.Groups["json"].Value;

				JSONRepresentation parsed = JSONHelper.Deserialize<JSONRepresentation>(json);

				parsed.Description = rawDescription.Substring(0, virtualBoxServiceTag.Index);
                return parsed;
			}
			else {
                //Service-Tag not found -> no Autostart, etc
                ParsedDescription d = new ParsedDescription();
                d.Autostart = false;
                return d;
			}
		}
	}
}

