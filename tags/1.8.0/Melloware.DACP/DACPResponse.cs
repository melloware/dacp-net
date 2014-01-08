/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Melloware.Core;
using log4net;

namespace Melloware.DACP {
	/// <summary>
	/// Abstract base class for all DACP Response messages.
	/// </summary>
	public abstract class DACPResponse {

		// constants
		public const int PLAYING = 4;
		public const int PAUSED = 3;
		public const int STOPPED = 2;
		
		public const string PROPERTY_VOLUME = "dmcp.volume";
		public const string PROPERTY_PLAYINGTIME = "dacp.playingtime";
		public const string PROPERTY_REPEATSTATE = "dacp.repeatstate";
		public const string PROPERTY_SHUFFLESTATE = "dacp.shufflestate";
		public const string PROPERTY_RATING = "dacp.userrating";
		public const string PROPERTY_VISUALIZER = "dacp.visualizer";
		public const string PROPERTY_FULLSCREEN = "dacp.fullscreen";
		public const string PROPERTY_ALBUMID = "daap.songalbumid";
		public const string PROPERTY_ALBUMNAME = "daap.songalbum";
		public const string PROPERTY_ARTISTNAME = "daap.songartist";
		public const string PROPERTY_GENRE = "daap.songgenre";
		public const string PROPERTY_COMPOSER = "daap.songcomposer";
		public const string PROPERTY_ITEMNAME = "dmap.itemname";
		public const string PROPERTY_ITEMID = "dmap.itemid";
		public const string PROPERTY_CONTAINERITEMID = "dmap.containeritemid";
		public const string PROPERTY_PERSISTENTID = "dmap.persistentid";
		public const string PROPERTY_PLAYERSTATE = "dacp.playerstate";
		public const string PROPERTY_MEDIAKIND = "com.apple.itunes.mediakind";
		public const string PROPERTY_VOTE = "com.apple.itunes.jukebox-vote";
		public const string PROPERTY_REVISION = "revision-number";
		public const string PROPERTY_QUERY = "query";
		public const string PROPERTY_FILTER = "filter";
		public const string PROPERTY_EDITPARAMS = "edit-params";
		public const string PROPERTY_EDITPARAM_MOVEPAIR = "edit-param.move-pair";
		public const string PROPERTY_META = "meta";
		public const string PROPERTY_TYPE = "type";
		public const string PROPERTY_SORT = "sort";
		public const string PROPERTY_INDEX = "index";
		public const string PROPERTY_SESSION = "session-id";
		public const string PROPERTY_CONTAINERS = "containers";
		public const string PROPERTY_PAIRING_GUID = "pairing-guid";
		public const string PROPERTY_CONTAINER_SPEC = "container-spec";
		public const string PROPERTY_CONTAINER_ITEM_SPEC = "container-item-spec";
		public const string PROPERTY_DATABASE_SPEC = "database-spec";
		public const string PROPERTY_ITEM_SPEC = "item-spec";
		public const string PROPERTY_SPEAKER_ID = "speaker-id";
		public const string PROPERTY_GROUP_TYPE = "group-type";

		public const string SORT_ARTIST = "artist";
		public const string SORT_ALBUM = "album";
		public const string SORT_TRACK = "name";

		public const int MSTT_OK = 200;
		public const int MSTT_CREATED = 201;
		public const int MSTT_NO_CONTENT = 204;
		public const int MSTT_NOT_FOUND = 404;
		public const int MSTT_ERROR = 500;

		public const byte TRUE = 1;
		public const byte FALSE = 0;
		
		// iTunes constants
		public const int MEDIAKIND_MUSIC = 1;
		public const int MEDIAKIND_VIDEO = 2;
		public const int MEDIAKIND_PODCAST = 4;
		public const int MEDIAKIND_PODCAST2 = 6;
		public const int MEDIAKIND_PODCAST3 = 36;
		public const int MEDIAKIND_AUDIOBOOK = 8;
		public const int MEDIAKIND_MUSICVIDEO = 32;
		public const int MEDIAKIND_TVSHOW = 64;

		// constants for DACP protocol
		public const string ABAR = "abar"; // #Browse Artist list
		public const string ABCP = "abcp"; // #Browse Composer list
		public const string ABGN = "abgn"; // #Browse Genre list
		public const string ABPL = "abpl"; // #daap base playlist indicator
		public const string ABRO = "abro"; // #Database Browse
		public const string AEFP = "aeFP"; // #Apple Itunes Unknown (Server Info)
		public const string AEFR = "aeFR"; // #Apple Itunes Unknown (CtrlInt Info)
		public const string AETR = "aeTr"; // #Apple Itunes Unknown (CtrlInt Info)
		public const string AEHV = "aeHV"; // #Apple Itunes Has Video flag (0 = no)
		public const string AEPS = "aePS"; // #Itunes special playlist
		public const string AEMK = "aeMK"; // #Itunes Extended Media Kind
		public const string AEMQ = "aeMQ"; // #Unknown
		public const string AESL = "aeSL"; // #Unknown
		public const string AESR = "aeSR"; // #Unknown
		public const string AESX = "aeSX"; // #Unknown
		public const string AESP = "aeSP"; // #Itunes smart playlist
		public const string AESV = "aeSV"; // #Apple Itunes Music Sharing Version
		public const string AGAL = "agal"; // #Album Grouping
		public const string AGAR = "agar"; // #Artist Grouping
		public const string AGAC = "agac"; // #Artist Album Count
		public const string APLY = "aply"; // #database playlists
		public const string APRO = "apro"; // #DAAP Protocol Version
		public const string APSO = "apso"; // #Song or Track Listing
		public const string ASAA = "asaa"; // #Album Artist
		public const string ASAI = "asai"; // #song album id
		public const string ASAL = "asal"; // #Song Album name
		public const string ASAR = "asar"; // #Song Artist name
		public const string ASCO = "asco"; // #Song Part of A Compilation
		public const string ASDB = "asdb"; // #Song Disabled
		public const string ASDN = "asdn"; // #Song Disc Number
		public const string ASGN = "asgn"; // #Song Genre
		public const string ASRI = "asri"; // #Song Artist ID
		public const string ASTM = "astm"; // #Song Time
		public const string ASTN = "astn"; // #Song Track Number
		public const string ASUR = "asur"; // Song User Rating (0-5)
		public const string ASSA = "assa"; // Song Sort Artist
		public const string ASSE = "asse"; // Unknown Server Response
		public const string ASSU = "assu"; // Song Sort Album
		public const string ASYR = "asyr"; // #Song Year
		public const string ATED = "ated"; // #Supports Extra Data
		public const string ASCN = "ascn"; // #iTunes Comment field, used to identify Radio Streams
		public const string ASGR = "asgr"; // #Use Groups
		public const string AVDB = "avdb"; // #server databases
		public const string CAAR = "caar"; // #available repeat states, only seen '6'
		public const string CAAS = "caas"; // #available shuffle states, only seen '2'
		public const string CACI = "caci"; // #CtrlInt Response
		public const string CAIA = "caia"; // #computer audio is availabe (1 == true)
		public const string CAIV = "caiv"; // #computer audio for videos is availabe (1 == true)
		public const string CAIP = "caip"; // #computer audio for videos is playing (1 == true)
		public const string CANA = "cana"; // #now playing artist
		public const string CANG = "cang"; // #now playing genre
		public const string CANL = "canl"; // #now playing album
		public const string CANN = "cann"; // #now playing track
		public const string CANP = "canp"; // #nowplaying 4 ids: dbid, plid, playlistItem, itemid
		public const string CANT = "cant"; // #song remaining time
		public const string CAPS = "caps"; // #play status: 4=playing, 3=paused, 2=stopped
		public const string CARP = "carp"; // #repeat status: 0=none, 1=single, 2=all
		public const string CASH = "cash"; // #shuffle status: 0=off, 1=on
		public const string CASP = "casp"; // #Computer Audio Speaker List
		public const string CASS = "cass"; // #CtrlInt ???
		public const string CAST = "cast"; // #song time
		public const string CASU = "casu"; // #CtrlInt ???
		public const string CAFE = "cafe"; // #Fullscreen Enabled: 0=false, 1=true
		public const string CAVE = "cave"; // #visualizer Enabled: 0=false, 1=true
		public const string CMPR = "cmpr"; // #CtrlInt ???
		public const string CAPR = "capr"; // #CtrlInt ???
		public const string CAOV = "caov"; // #CtrlInt ???
		public const string CAVC = "cavc"; // #volume controllable: 0=false, 1=true
		public const string CAVS = "cavs"; // #visualizer controllable: 0=false, 1=true
		public const string CAFS = "cafs"; // #fullscreen controllable: 0=false, 1=true
		public const string CESG = "ceSG"; // #CtrlInt ???
		public const string CEGS = "ceGS"; // #Genius Seed Selectable
		public const string CEVO = "ceVO"; // Unknown Server Info
		public const string CEWM = "ceWM"; // Unknown Server Info
		public const string CEJI = "ceJI"; // Itunes DJ Jukebox Current
		public const string CEJV = "ceJV"; // Itunes DJ Jukebox Vote
		public const string CMGT = "cmgt"; // #getproperty response
		public const string CMIK = "cmik"; // #CtrlInt ???
		public const string CMMK = "cmmk"; // #media kind
		public const string CMNM = "cmnm"; // #pairing client devicename
		public const string CMPA = "cmpa"; // #pairing client response
		public const string CMPG = "cmpg"; // #pairing client GUID
		public const string CMRL = "cmrl"; // #Apple Itunes Unknown (CtrlInt Info)
		public const string CMSP = "cmsp"; // #CtrlInt ???
		public const string CMSR = "cmsr"; // #media revision
		public const string CMST = "cmst"; // #status
		public const string CMSV = "cmsv"; // #CtrlInt ???
		public const string CMTY = "cmty"; // #pairing client device type
		public const string CMVO = "cmvo"; // #volume
		public const string MCTC = "mctc"; // #container count
		public const string MCTI = "mcti"; // #container item id
		public const string MDBK = "mdbk"; // #database kind (local itunes database = 1)
		public const string MDCL = "mdcl"; // #speaker list
		public const string MEDC = "medc"; // #edit playlist/dictionary
		public const string MEDS = "meds"; // #edit status
		public const string MIID = "miid"; // #unique item id
		public const string MIKD = "mikd"; // #Item Kind (song = 1)
		public const string MIMC = "mimc"; // #item count
		public const string MINM = "minm"; // #name
		public const string MLCL = "mlcl"; // #listing
		public const string MLID = "mlid"; // #session id
		public const string MLIT = "mlit"; // #listing item
		public const string MLOG = "mlog"; // #login response
		public const string MPCO = "mpco"; // #parent container id
		public const string MPER = "mper"; // #persistent id
		public const string MPRO = "mpro"; // #DMAP Protocol Version
		public const string MRCO = "mrco"; // #number of items returned here
		public const string MSAL = "msal"; // #Support Auto Logout
		public const string MSAS = "msas"; // #Support Authentication Schemes
		public const string MSBR = "msbr"; // #Support Browse
		public const string MSDC = "msdc"; // #Database Count
		public const string MSED = "msed"; // #Supports Edit
		public const string MSEX = "msex"; // #Support Extensions
		public const string MSHC = "mshc"; // Browse Index Alphabetic Letter
		public const string MSHI = "mshi"; // Browse Index Order
		public const string MSHL = "mshl"; // Browse Index Node List Response
		public const string MSHN = "mshn"; // Browse Item Count For This Letter
		public const string MSIX = "msix"; // #Support Index
		public const string MSLR = "mslr"; // #Login Required
		public const string MSML = "msml"; // #Speaker Machine List
		public const string MSMA = "msma"; // #speaker machine address (only seen 0)
		public const string MSPI = "mspi"; // #Support Persistent IDs
		public const string MSQY = "msqy"; // #Support Query
		public const string MSRS = "msrs"; // #Support Resolve
		public const string MSRV = "msrv"; // #Server Info Response
		public const string MSTC = "mstc"; // #UTC Time (in seconds)
		public const string MSTM = "mstm"; // #Timeout Interval
		public const string MSTO = "msto"; // #UTC Time Offset (in seconds)
		public const string MSTT = "mstt"; // #status (200 = OK)
		public const string MSUP = "msup"; // #Support Updates
		public const string MTCO = "mtco"; // #total items found
		public const string MUDL = "mudl"; // #Deleted ID Listing
		public const string MUPD = "mupd"; // #Server Update Response
		public const string MUSR = "musr"; // #Server Revision Number
		public const string MUTY = "muty"; // #update type

		// optimization for calculating buffer's
		private static readonly int BUFFER_BYTE = Endian.ConvertInt32(1);
		private static readonly int BUFFER_SHORT = Endian.ConvertInt32(2);
		private static readonly int BUFFER_INT = Endian.ConvertInt32(4);
		private static readonly int BUFFER_LONG = Endian.ConvertInt32(8);
		private static readonly int BUFFER_ARRAY = Endian.ConvertInt32(16);

		// regex will match any string in single quotes.
		private static readonly Regex regexSingleQuotes = new Regex(@"'(?:\.|(\\\')|[^\''\n])*'", RegexOptions.IgnoreCase|RegexOptions.Compiled);

		// logger
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// fields
		private int mstt = MSTT_OK; // #status (200 = OK)
		private bool isIndexed = false;
		private int startIndex = 0;
		private int endIndex = 50;
		private readonly NameValueCollection queryParams = new NameValueCollection();



		/// <summary>
		/// Holds the incoming HTTP Request for use to construct this response
		/// </summary>
		private HttpListenerRequest httpRequest;
		
		/// <summary>
		/// Named value pairs of Query string but UTF-8 decoded.  Cannot use Request.QueryString
		/// http://blog.mischel.com/2011/01/12/httplistenercontext-and-url-encoded-query-parameters/
		/// </summary>
		private NameValueCollection QueryCollection;

		/// <summary>
		/// Default empty constructor.
		/// Protected level so only subclasses can use it.
		/// </summary>
		protected DACPResponse() {
			// empty constructor
		}

		/// <summary>
		/// Constructor that takes an HttpListenerRequest.
		/// </summary>
		/// <param name="request">the HttpListenerRequest</param>
		public DACPResponse(HttpListenerRequest request) {
			if (request == null) {
				return;
			}

			try {
				this.HttpRequest = request;
				
				QueryCollection = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
				foreach ( String s in QueryCollection.AllKeys ) {
					LOG.DebugFormat( "    Param: {0,-10} {1}", s, QueryCollection[s] );
				}
				
				this.StartIndex = GetQueryIndexStart();
				this.EndIndex = GetQueryIndexEnd();

				// (('com.apple.itunes.mediakind:1','com.apple.itunes.mediakind:4','com.apple.itunes.mediakind:8') ('dmap.itemname:*Eulogy*','daap.songartist:*Eulogy*','daap.songalbum:*Eulogy*'))
				ExtractParameters(GetQuery());
				ExtractParameters(GetFilter());
				ExtractParameters(GetEditParams());

			} catch (Exception ex) {
				LOG.Error("DACPResponse error getting HTTP properties: " + ex.Message, ex);
			}
		}

		/// <summary>
		/// Extracts the parameters from either filter or query string.
		/// </summary>
		/// <param name="property">the property to extract the params</param>
		private void ExtractParameters(string property) {
			if (property != null) {
				//first check for a single property like daap.songartist:Tool
				string[] colons = property.Split(':');
				if (colons.Length == 2) {
					//found only one
					ExtractParam(property);
				} else {
					Match m;
					// find all matches in between single quotes
					for (m = regexSingleQuotes.Match(property); m.Success; m = m.NextMatch()) {
						//LOG.DebugFormat("Match {0} = {1}",m.Value, m.Index);
						ExtractParam(m.Value);
					}
				}
			}
		}

		/// <summary>
		/// For this property split it into its column and value
		/// components.  So daap.songartist:Tool becomes
		/// column = daap.songartist
		/// value = Tool
		/// </summary>
		/// <param name="property">the property to extract</param>
		private void ExtractParam(string property) {
			string[] values = property.Split(':');
			if (values.Length == 2) {
				string queryCol = values[0].Trim();
				string queryVal = values[1].Trim();
				
				// skip if neither value is filled out
				if (queryCol.Length == 0 || queryVal.Length == 0) {
					return;
				}

				// remove the first character which is a quote
				queryCol = queryCol.Remove(0, 1);

				// remove the last character which is a quote
				queryVal = queryVal.Remove(queryVal.Length -1);
				queryVal = queryVal.Replace("\\", "");
				queryVal = queryVal.Replace("'", "''");
				queryVal = queryVal.Replace("*", "%");
				
				// if the value starts with 0x it is hex and convert it to Int64
				if (queryVal.StartsWith("0x")) {
					try {
						queryVal = queryVal.Remove(0,2);
						UInt64 iValue = UInt64.Parse(queryVal, System.Globalization.NumberStyles.HexNumber);
						queryVal = iValue.ToString();
					} catch (Exception ex) {
						LOG.Error("DACPResponse error converting Hex property in ExtractParam: " + ex.Message, ex);
					}
				}

				LOG.DebugFormat("    Query Param: {0} = {1}",queryCol, queryVal);
				// add to the NamedValueCollection
				queryParams.Add(queryCol, queryVal);
			}
		}

		/// <summary>
		/// Abstract method to represent this DACPResposne in bytes
		/// </summary>
		/// <returns>a byte[] array containing the binary response</returns>
		public abstract byte[] GetBytes();

		/// <summary>
		/// Looks in the properties field and splits on the '=' sign returning
		/// the right hand value.
		///
		/// Example: dmcp.volume=49 returns 49
		/// </summary>
		/// <param name="property">the property to get the value for</param>
		/// <returns></returns>
		public static string GetProperty(string property) {
			string result = String.Empty;
			string[] values = property.Split('=');
			if (values.Length > 1) {
				result = values[1];
			}
			return result;
		}

		/// <summary>
		/// Creates the full DACP response with the root element and the payload.
		///
		/// mlog --+
		///     mstt 4 000000c8 == 200
		///     mlid 4 648a861f == 1686799903 # our new session-id
		/// </summary>
		/// <param name="rootElement">the Root document element like mlog</param>
		/// <param name="payloadBytes">the entire payload of the response</param>
		/// <returns></returns>
		protected static byte[] CreateDACPResponseBytes(string rootElement, byte[] payloadBytes) {
			byte[] output = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));

				// write the root element first
				writer.Write(new UTF8Encoding(false).GetBytes(rootElement));

				// get the size of the whole payload we are writing out
				writer.Write(Endian.ConvertInt32(payloadBytes.Length));

				// write the payload out.
				writer.Write(payloadBytes, 0, payloadBytes.Length);

				writer.Flush();
				output = stream.ToArray();
			}

			return output;
		}

		/// <summary>
		/// Converts a DACP string value into a byte[] array.
		///
		/// Example: cmty 4 648a861f == ipod
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like CMTY</param>
		/// <param name="strValue">the string value to set to this key</param>
		protected static void StreamString(BinaryWriter stream, string key, string strValue) {
			if (strValue == null) {
				return;
			}
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			byte[] utf8Value = new UTF8Encoding(false).GetBytes(strValue);
			stream.Write(Endian.ConvertInt32(utf8Value.Length));
			stream.Write(utf8Value, 0 , utf8Value.Length);
		}

		/// <summary>
		/// Converts a DACP integer value into a byte[] array.
		///
		/// Example: mstt 4 000000c8 == 200
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like MSTT</param>
		/// <param name="intValue">the integer value to set to this key</param>
		protected static void StreamInteger(BinaryWriter stream, string key, int intValue) {
			if (intValue < 0) {
				return;
			}
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			stream.Write(BUFFER_INT);
			stream.Write(Endian.ConvertInt32(intValue));
		}

		/// <summary>
		/// Converts a DACP short value into a byte[] array.
		///
		/// Example: muty   1      00 == 0
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like MUTY</param>
		/// <param name="intValue">the short value to set to this key</param>
		protected static void StreamShort(BinaryWriter stream, string key, ushort shortValue) {
			if (shortValue < 0) {
				return;
			}
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			stream.Write(BUFFER_SHORT);
			stream.Write(Endian.ConvertUInt16(shortValue));
		}

		/// <summary>
		/// Converts a DACP byte value into a byte[] array.
		///
		/// Example: muty   1      00 == 0
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like MUTY</param>
		/// <param name="intValue">the short value to set to this key</param>
		protected static void StreamByte(BinaryWriter stream, string key, byte byteValue) {
			if (byteValue < 0) {
				return;
			}
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			stream.Write(BUFFER_BYTE);
			stream.Write(byteValue);
		}

		/// <summary>
		/// Converts a DACP ulong value into a byte[] array.
		///
		/// Example: mper   8      d19bb75c3773b487 == 15103867382012294279
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like MPER</param>
		/// <param name="intValue">the ulong value to set to this key</param>
		protected static void StreamLong(BinaryWriter stream, string key, ulong longValue) {
			if (longValue < 0) {
				return;
			}
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			stream.Write(BUFFER_LONG);
			stream.Write(Endian.ConvertUInt64(longValue));
		}

		/// <summary>
		/// Converts an int[] array into a 16 byte DACP record. So far
		/// this is only used for CANP of the PlayerStatusUpdate.
		/// </summary>
		/// <param name="stream">the MemoryStream to write the buffer to</param>
		/// <param name="key">the key value like CANP</param>
		/// <param name="array">the integer array of values</param>
		protected static void StreamIntArray(BinaryWriter stream, string key, int[] array) {
			stream.Write(new UTF8Encoding(false).GetBytes(key));
			stream.Write(BUFFER_ARRAY);
			foreach (int arrayValue in array) {
				stream.Write(Endian.ConvertInt32(arrayValue));
			}
		}

		/// <summary>
		/// Converts a Hex query parameter to its ulong equivalent.
		///
		/// Example: /login?pairing-guid=0x0000000000000001
		///
		/// The guid is converted to ulong = 1;
		/// </summary>
		/// <param name="request">The HTTP Request to get the URL param</param>
		/// <param name="property">the parameter to get</param>
		/// <returns> a single ulong representing the value</returns>
		public static ulong ConvertHexParameterToLong(HttpListenerRequest request, string parameter) {
			ulong result = 0;
			try {
				string propertyValue = request.QueryString[parameter];
				propertyValue = propertyValue.Replace("'", "");
				LOG.Debug(propertyValue);
				string[] values = propertyValue.Split(':');
				if (values.Length == 2) {
					propertyValue = values[1];
				}
				propertyValue = propertyValue.Remove(0,2);
				result = UInt64.Parse(propertyValue, System.Globalization.NumberStyles.HexNumber);
				LOG.DebugFormat("Hex Property '{0}' = {1}", parameter, result);
			} catch (Exception) {
				result = 0;
			}
			return result;
		}

		/// <summary>
		/// Converts an array of Hex params to an array of Int64's.
		/// </summary>
		/// <param name="request">The HTTP Request to get the URL param</param>
		/// <param name="property">the parameter to get</param>
		/// <returns>an array of ulong representing the values</returns>
		public static ulong[] ConvertHexParameterToInt64Array(HttpListenerRequest request, string parameter) {
			ArrayList result = new ArrayList();
			try {
				string propertyValue = request.QueryString[parameter];
				propertyValue = propertyValue.Replace("'", "");
				LOG.Debug(propertyValue);
				string[] values = propertyValue.Split(':');
				if (values.Length == 2) {
					propertyValue = values[1];
				}
				values = propertyValue.Split(',');
				if (values.Length > 1) {
					for (int i = 0; i < values.Length; i++) {
						string hexProperty = values[i];
						LOG.DebugFormat("Hex Property '{0}' = {1}", parameter, hexProperty);
						if (hexProperty.StartsWith("0x")) {
							hexProperty = hexProperty.Remove(0,2);
						}
						result.Add(UInt64.Parse(hexProperty, System.Globalization.NumberStyles.HexNumber));
					}
				} else {
					string hexProperty = propertyValue;
					LOG.DebugFormat("Hex Property '{0}' = {1}", parameter, hexProperty);
					if (hexProperty.StartsWith("0x")) {
						hexProperty = hexProperty.Remove(0,2);
					}
					result.Add(UInt64.Parse(hexProperty, System.Globalization.NumberStyles.HexNumber));
				}

			} catch (Exception ex) {
				LOG.Error("ConvertHexParameterToInt64Array Exception", ex);
				result.Add(0); //default to Computer speakers
			}
			return (ulong[]) result.ToArray(typeof(ulong));
		}

		/// <summary>
		/// Converts a Hex query parameter to its ulong equivalent.
		///
		/// Example: /login?pairing-guid=0x0000000000000001
		///
		/// The guid is converted to ulong = 1;
		/// </summary>
		/// <param name="request">The HTTP Request to get the URL param</param>
		/// <param name="property">the parameter to get</param>
		/// <returns></returns>
		public static int ConvertHexParameterToInt(HttpListenerRequest request, string parameter) {
			int result = 0;
			try {
				string propertyValue = request.QueryString[parameter];
				propertyValue = propertyValue.Replace("'", "");
				string[] values = propertyValue.Split(':');
				if (values.Length == 2) {
					propertyValue = values[1];
				}
				propertyValue = propertyValue.Remove(0,2);
				result = Int32.Parse(propertyValue, System.Globalization.NumberStyles.HexNumber);
				LOG.DebugFormat("Hex Property '{0}' = {1}", parameter, result);
			} catch (Exception) {
				result = 0;
			}
			return result;
		}

		public string GetQueryMeta() {
			return this.QueryCollection[PROPERTY_META];
		}

		public string GetQueryType() {
			return this.QueryCollection[PROPERTY_TYPE];
		}

		public string GetQuerySort() {
			return this.QueryCollection[PROPERTY_SORT];
		}

		public string GetQuery() {
			return this.QueryCollection[PROPERTY_QUERY];
		}

		public string GetFilter() {
			return this.QueryCollection[PROPERTY_FILTER];
		}

		public string GetEditParams() {
			return this.QueryCollection[PROPERTY_EDITPARAMS];
		}

		public int GetQueryIndex() {
			int result = 0;
			try {
				result = Convert.ToInt32(this.QueryCollection[PROPERTY_INDEX]);
			} catch (Exception) {
				result = 0;
			}
			return result;
		}

		public int GetQueryIndexStart() {
			string index = this.QueryCollection[PROPERTY_INDEX];
			int result = 0;
			try {
				if (index != null) {
					string[] values = index.Split('-');
					if (values.Length == 2) {
						this.IsIndexed = true;
						result = Convert.ToInt32(values[0]);
					}
				}
			} catch (Exception) {
				result = 0;
			}

			return result;
		}

		public int GetQueryIndexEnd() {
			string index = this.QueryCollection[PROPERTY_INDEX];
			int result = 50;
			try {
				if (index != null) {
					string[] values = index.Split('-');
					if (values.Length == 2) {
						this.IsIndexed = true;
						result = Convert.ToInt32(values[1]);
					}
				}
			} catch (Exception) {
				result = 50;
			}
			return result;
		}

		public int GetContainerId() {
			int result = 1;
			// check whether this is playlist request for tracks
			if (this.HttpRequest.RawUrl.Contains(PROPERTY_CONTAINERS)) {
				try {
					string[] values = HttpRequest.RawUrl.Split('/');
					LOG.Debug(values);
					// the 5th item is the id (/databases/1/containers/2854/)
					result = Convert.ToInt32(values[4]);
				} catch (Exception ex) {
					LOG.Warn("Error trying to get the container id so defaulting.", ex);
				}
			}
			return result;
		}

		/// <summary>
		/// The HTTP Request used to create this reponse
		/// </summary>
		public HttpListenerRequest HttpRequest {
			get {
				return httpRequest;
			} set {
				httpRequest = value;
			}
		}

		/// <summary>
		/// If found the starting Index for a search
		/// </summary>
		public int StartIndex {
			get {
				return startIndex;
			} set {
				startIndex = value;
			}
		}

		/// <summary>
		/// If found the Ending index for a search
		/// </summary>
		public int EndIndex {
			get {
				return endIndex;
			} set {
				endIndex = value;
			}
		}

		/// <summary>
		/// The URL Query params as a NameValueCollection
		/// </summary>
		public NameValueCollection QueryParams {
			get {
				return queryParams;
			}
		}

		/// <summary>
		/// DACP Response Status Code. 200 = OK
		/// </summary>
		public int Mstt {
			get {
				return mstt;
			} set {
				mstt = value;
			}
		}

		/// <summary>
		/// DACP Update Type
		/// </summary>
		protected byte Muty {
			get;    // #update type
			set;
		}

		/// <summary>
		/// Total items returned count
		/// </summary>
		protected int Mtco {
			get;    // #total items found
			set;
		}

		/// <summary>
		/// Number of items returned in this batch
		/// </summary>
		protected int Mrco {
			get;    // #number of items returned here
			set;
		}

		/// <summary>
		/// Determines if an index was sent in like index=0-7.
		/// </summary>
		public bool IsIndexed {
			get {
				return isIndexed;
			} set {
				isIndexed = value;
			}
		}
	}
}
