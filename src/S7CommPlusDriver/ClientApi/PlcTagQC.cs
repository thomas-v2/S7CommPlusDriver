namespace S7CommPlusDriver.ClientApi
{
	// Tag Quality Codes, based on OPC DA
	// Quality and Sub status: QQSSSS
	public static class PlcTagQC
	{
		// Bit masks for Quality, Status and Limits
		public const short TAG_QUALITY_MASK = 0xC0;
		public const short TAG_STATUS_MASK = 0xFC;
		public const short TAG_LIMIT_MASK = 0x03;

		// Values for Quality
		public const short TAG_QUALITY_BAD = 0x00;
		public const short TAG_QUALITY_UNCERTAIN = 0x40;
		public const short TAG_QUALITY_GOOD = 0xC0;

		// Status bits when Quality = BAD
		public const short TAG_QUALITY_CONFIG_ERROR = 0x04;
		public const short TAG_QUALITY_NOT_CONNECTED = 0x08;
		public const short TAG_QUALITY_DEVICE_FAILURE = 0x0c;
		public const short TAG_QUALITY_SENSOR_FAILURE = 0x10;
		public const short TAG_QUALITY_LAST_KNOWN = 0x14;
		public const short TAG_QUALITY_COMM_FAILURE = 0x18;
		public const short TAG_QUALITY_OUT_OF_SERVICE = 0x1C;
		public const short TAG_QUALITY_WAITING_FOR_INITIAL_DATA = 0x20;

		// Status bits when Quality = UNCERTAIN
		public const short TAG_QUALITY_LAST_USABLE = 0x44;
		public const short TAG_QUALITY_SENSOR_CAL = 0x50;
		public const short TAG_QUALITY_EGU_EXCEEDED = 0x54;
		public const short TAG_QUALITY_SUB_NORMAL = 0x58;

		// Status bits when Quality = GOOD
		public const short TAG_QUALITY_LOCAL_OVERRIDE = 0xD8;

		// Status bits for Limit Bitfield 
		public const short TAG_LIMIT_OK = 0x00;
		public const short TAG_LIMIT_LOW = 0x01;
		public const short TAG_LIMIT_HIGH = 0x02;
		public const short TAG_LIMIT_CONST = 0x03;
	}
}
