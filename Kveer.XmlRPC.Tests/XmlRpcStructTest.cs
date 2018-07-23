using System;
using System.Collections;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[TestFixture]
	public class XmlRpcStructTest
	{
		[Test]
		public void Add()
		{
			var xps = new XmlRpcStruct();
			xps.Add("foo", "abcdef");
			Assert.AreEqual("abcdef", xps["foo"]);
		}

		[Test]
		public void AddInvalidKey()
		{
			var xps = new XmlRpcStruct();
			Assert.That(() => xps.Add(1, "abcdef"), Throws.ArgumentException);
		}

		[Test]
		public void DoubleAdd()
		{
			var xps = new XmlRpcStruct();
			xps.Add("foo", "123456");
			Assert.That(() => xps.Add("foo", "abcdef"), Throws.ArgumentException);
		}

		[Test]
		public void DoubleSet()
		{
			var xps = new XmlRpcStruct();
			xps["foo"] = "12345";
			xps["foo"] = "abcdef";
			Assert.AreEqual("abcdef", xps["foo"]);
		}

		[Test]
		public void OrderingEnumerator()
		{
			var xps = new XmlRpcStruct();
			xps.Add("1", "a");
			xps.Add("3", "c");
			xps.Add("2", "b");
			var enumerator = xps.GetEnumerator();
			enumerator.MoveNext();
			Assert.AreEqual("1", enumerator.Key);
			Assert.AreEqual("a", enumerator.Value);
			Assert.IsInstanceOf<DictionaryEntry>(enumerator.Current);
			var de = (DictionaryEntry) enumerator.Current;
			Assert.AreEqual("1", de.Key);
			Assert.AreEqual("a", de.Value);
			enumerator.MoveNext();
			Assert.AreEqual("3", enumerator.Key);
			Assert.AreEqual("c", enumerator.Value);
			Assert.IsInstanceOf<DictionaryEntry>(enumerator.Current);
			de = (DictionaryEntry) enumerator.Current;
			Assert.AreEqual("3", de.Key);
			Assert.AreEqual("c", de.Value);
			enumerator.MoveNext();
			Assert.AreEqual("2", enumerator.Key);
			Assert.AreEqual("b", enumerator.Value);
			Assert.IsInstanceOf<DictionaryEntry>(enumerator.Current);
			de = (DictionaryEntry) enumerator.Current;
			Assert.AreEqual("2", de.Key);
			Assert.AreEqual("b", de.Value);
		}

		[Test]
		public void OrderingKeys()
		{
			var xps = new XmlRpcStruct();
			xps.Add("1", "a");
			xps.Add("3", "c");
			xps.Add("2", "b");

			var enumerator = xps.Keys.GetEnumerator();
			enumerator.MoveNext();
			Assert.AreEqual("1", enumerator.Current);
			enumerator.MoveNext();
			Assert.AreEqual("3", enumerator.Current);
			enumerator.MoveNext();
			Assert.AreEqual("2", enumerator.Current);
		}

		[Test]
		public void OrderingValues()
		{
			var xps = new XmlRpcStruct();
			xps.Add("1", "a");
			xps.Add("3", "c");
			xps.Add("2", "b");

			var enumerator = xps.Values.GetEnumerator();
			enumerator.MoveNext();
			Assert.AreEqual("a", enumerator.Current);
			enumerator.MoveNext();
			Assert.AreEqual("c", enumerator.Current);
			enumerator.MoveNext();
			Assert.AreEqual("b", enumerator.Current);
		}

		[Test]
		public void Set()
		{
			var xps = new XmlRpcStruct();
			xps["foo"] = "abcdef";
			Assert.AreEqual("abcdef", xps["foo"]);
		}

		[Test]
		public void SetInvalidKey()
		{
			var xps = new XmlRpcStruct();
			Assert.That(() => xps[1] = "abcdef", Throws.ArgumentException);
		}
	}
}