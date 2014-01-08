/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Runtime.Serialization;

namespace Melloware.DACP
{
	/// <summary>
	/// If any Bonjour related error or Bonjour is not installed.
	/// </summary>
	public class DACPBonjourException : Exception, ISerializable
	{
		public DACPBonjourException()
		{
		}

	 	public DACPBonjourException(string message) : base(message)
		{
		}

		public DACPBonjourException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected DACPBonjourException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}