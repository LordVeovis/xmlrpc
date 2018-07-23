using System;
using System.Text;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[TestFixture]
	public class OptionalSerializeTest
	{
		private struct ChildStruct
		{
			public readonly int x;

			public ChildStruct(int num)
			{
				x = num;
			}
		}

		private struct Struct0
		{
			public XmlRpcInt      xi;
			public XmlRpcBoolean  xb;
			public XmlRpcDouble   xd;
			public XmlRpcDateTime xdt;
#if !FX1_0
			public int?         nxi;
			public bool?        nxb;
			public double?      nxd;
			public DateTime?    nxdt;
			public ChildStruct? nxstr;
#endif
		}

		private struct Struct1
		{
			public int            mi;
			public string         ms;
			public bool           mb;
			public double         md;
			public DateTime       mdt;
			public byte[]         mb64;
			public int[]          ma;
			public XmlRpcInt      xi;
			public XmlRpcBoolean  xb;
			public XmlRpcDouble   xd;
			public XmlRpcDateTime xdt;
			public XmlRpcStruct   xstr;
#if !FX1_0
			public int?         nxi;
			public bool?        nxb;
			public double?      nxd;
			public DateTime?    nxdt;
			public ChildStruct? nxstr;
#endif
		}

		[XmlRpcMissingMapping(MappingAction.Error)]
		private struct Struct2
		{
			public int            mi;
			public string         ms;
			public bool           mb;
			public double         md;
			public DateTime       mdt;
			public byte[]         mb64;
			public int[]          ma;
			public XmlRpcInt      xi;
			public XmlRpcBoolean  xb;
			public XmlRpcDouble   xd;
			public XmlRpcDateTime xdt;
			public XmlRpcStruct   xstr;
#if !FX1_0
			public int?         nxi;
			public bool?        nxb;
			public double?      nxd;
			public DateTime?    nxdt;
			public ChildStruct? nxstr;
#endif
		}

		private struct Struct3
		{
			[XmlRpcMissingMapping(MappingAction.Error)]
			public int mi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public string ms;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public bool mb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public double md;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public DateTime mdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public byte[] mb64;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public int[] ma;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcInt xi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcBoolean xb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcDouble xd;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcDateTime xdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcStruct xstr;
#if !FX1_0
			[XmlRpcMissingMapping(MappingAction.Error)]
			public int? nxi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public bool? nxb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public double? nxd;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public DateTime? nxdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public ChildStruct? nxstr;
#endif
		}

		[XmlRpcMissingMapping(MappingAction.Ignore)]
		private struct Struct4
		{
			[XmlRpcMissingMapping(MappingAction.Error)]
			public int mi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public string ms;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public bool mb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public double md;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public DateTime mdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public byte[] mb64;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public int[] ma;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcInt xi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcBoolean xb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcDouble xd;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcDateTime xdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public XmlRpcStruct xstr;
#if !FX1_0
			[XmlRpcMissingMapping(MappingAction.Error)]
			public int? nxi;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public bool? nxb;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public double? nxd;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public DateTime? nxdt;

			[XmlRpcMissingMapping(MappingAction.Error)]
			public ChildStruct? nxstr;
#endif
		}

		[XmlRpcMissingMapping(MappingAction.Ignore)]
		private struct Struct5
		{
			public int            mi;
			public string         ms;
			public bool           mb;
			public double         md;
			public DateTime       mdt;
			public byte[]         mb64;
			public int[]          ma;
			public XmlRpcInt      xi;
			public XmlRpcBoolean  xb;
			public XmlRpcDouble   xd;
			public XmlRpcDateTime xdt;
			public XmlRpcStruct   xstr;
#if !FX1_0
			public int?         nxi;
			public bool?        nxb;
			public double?      nxd;
			public DateTime?    nxdt;
			public ChildStruct? nxstr;
#endif
		}

		private struct Struct6
		{
			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int mi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public string ms;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public bool mb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public double md;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public DateTime mdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public byte[] mb64;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int[] ma;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcInt xi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcBoolean xb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcDouble xd;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcDateTime xdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcStruct xstr;
#if !FX1_0
			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int? nxi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public bool? nxb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public double? nxd;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public DateTime? nxdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public ChildStruct? nxstr;
#endif
		}

		[XmlRpcMissingMapping(MappingAction.Error)]
		private struct Struct7
		{
			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int mi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public string ms;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public bool mb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public double md;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public DateTime mdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public byte[] mb64;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int[] ma;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcInt xi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcBoolean xb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcDouble xd;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcDateTime xdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcStruct xstr;
#if !FX1_0
			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public int? nxi;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public bool? nxb;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public double? nxd;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public DateTime? nxdt;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public ChildStruct? nxstr;
#endif
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct0_AllExist()
		{
			var strout = new Struct0();
			strout.xi  = 1234;
			strout.xb  = true;
			strout.xd  = 1234.567;
			strout.xdt = new DateTime(2006, 8, 9, 10, 11, 13);
#if !FX1_0
			strout.nxi   = 5678;
			strout.nxb   = true;
			strout.nxd   = 2345.678;
			strout.nxdt  = new DateTime(2007, 9, 10, 11, 12, 14);
			strout.nxstr = new ChildStruct(567);
#endif
			var xdoc = Utils.Serialize("Struct0_AllExist",
									   strout, Encoding.UTF8, MappingAction.Error);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, typeof(Struct0), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsInstanceOf<Struct0>(obj);
			var strin = (Struct0) obj;
			Assert.AreEqual(strout.xi, strin.xi);
			Assert.AreEqual(strout.xb, strin.xb);
			Assert.AreEqual(strout.xd, strin.xd);
			Assert.AreEqual(strout.xdt, strin.xdt);
#if !FX1_0
			Assert.AreEqual(strout.nxi, strin.nxi);
			Assert.AreEqual(strout.nxb, strin.nxb);
			Assert.AreEqual(strout.nxd, strin.nxd);
			Assert.AreEqual(strout.nxdt, strin.nxdt);
			Assert.AreEqual(((ChildStruct) strout.nxstr).x, ((ChildStruct) strin.nxstr).x);
#endif
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct1_AllMissing_ErrorDefault()
		{
			Assert.That(() => Utils.Serialize("Struct1_AllMissing_ErrorDefault",
											  new Struct1(),
											  Encoding.UTF8, MappingAction.Error),
						Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		[Test]
		public void Struct1_AllMissing_IgnoreDefault()
		{
			var xdoc = Utils.Serialize("Struct1_AllMissing_IgnoreDefault",
									   new Struct1(),
									   Encoding.UTF8, MappingAction.Ignore);
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct2_AllMissing_ErrorError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct2_AllMissing_ErrorError",
							new Struct2(),
							Encoding.UTF8, MappingAction.Error), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		[Test]
		public void Struct2_AllMissing_IgnoreError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct2_AllMissing_IgnoreError",
							new Struct2(),
							Encoding.UTF8, MappingAction.Ignore), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct3_AllMissing_ErrorDefaultError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct3_AllMissing_ErrorDefaultError",
							new Struct3(),
							Encoding.UTF8, MappingAction.Error), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		[Test]
		public void Struct3_AllMissing_IgnoreDefaultError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct3_AllMissing_IgnoreDefaultError",
							new Struct3(),
							Encoding.UTF8, MappingAction.Ignore), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct4_AllMissing_ErrorIgnoreError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct4_AllMissing_ErrorIgnoreError",
							new Struct4(),
							Encoding.UTF8, MappingAction.Error), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		[Test]
		public void Struct4_AllMissing_IgnoreIgnoreError()
		{
			Assert.That(() => Utils.Serialize(
							"Struct4_AllMissing_IgnoreIgnoreError",
							new Struct4(),
							Encoding.UTF8, MappingAction.Ignore), Throws.TypeOf<XmlRpcMappingSerializeException>());
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct5_AllMissing_ErrorIgnoreDefault()
		{
			var xdoc = Utils.Serialize(
				"Struct5_AllMissing_ErrorIgnoreDefault",
				new Struct5(),
				Encoding.UTF8, MappingAction.Error);
		}

		[Test]
		public void Struct5_AllMissing_IgnoreIgnoreDefault()
		{
			var xdoc = Utils.Serialize(
				"Struct5_AllMissing_IgnoreIgnoreDefault",
				new Struct5(),
				Encoding.UTF8, MappingAction.Ignore);
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct6_AllMissing_ErrorDefaultIgnore()
		{
			var xdoc = Utils.Serialize(
				"Struct6_AllMissing_ErrorDefaultIgnore",
				new Struct6(),
				Encoding.UTF8, MappingAction.Error);
		}

		[Test]
		public void Struct6_AllMissing_IgnoreDefaultIgnore()
		{
			var xdoc = Utils.Serialize(
				"Struct6_AllMissing_IgnoreDefaultIgnore",
				new Struct6(),
				Encoding.UTF8, MappingAction.Ignore);
		}

		//-------------------------------------------------------------------------/
		[Test]
		public void Struct7_AllMissing_ErrorErrorIgnore()
		{
			var xdoc = Utils.Serialize(
				"Struct7_AllMissing_ErrorErrorIgnore",
				new Struct7(),
				Encoding.UTF8, MappingAction.Error);
		}

		[Test]
		public void Struct7_AllMissing_IgnoreErrorIgnore()
		{
			var xdoc = Utils.Serialize(
				"Struct7_AllMissing_IgnoreErrorIgnore",
				new Struct7(),
				Encoding.UTF8, MappingAction.Ignore);
		}
	}
}