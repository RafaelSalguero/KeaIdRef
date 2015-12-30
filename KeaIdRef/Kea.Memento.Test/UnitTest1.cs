using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
namespace Kea.Memento.Test
{
    [TestClass]
    public class Memento
    {
        /// <summary>
        /// Attribute for properties that should be copied by the memento
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        sealed class MementoAttribute : Attribute
        {
            public MementoAttribute()
            {
            }
        }

        public class Model
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public int FullNameExecutionCount;
            public virtual string FullName
            {
                get
                {
                    FullNameExecutionCount++;
                    return FirstName + " " + LastName;
                }
            }
        }

        [TestMethod]
        public void MementoTest()
        {
            var Mod = new Model { FirstName = "Rafael", LastName = "Salguero" };

            var Memento = MementoFactory.Create(Mod, MementoFactory.SerializationInclude);

            //FullName property is accessed once by the memento factory:
            Assert.AreEqual(1, Mod.FullNameExecutionCount);

            //Menento.Instance constains now a copy of Mod

            Assert.AreEqual(Mod.FirstName, Memento.Instance.FirstName);
            Assert.AreEqual(Mod.LastName, Memento.Instance.LastName);

            Assert.AreEqual(Mod.FirstName + " " + Mod.LastName, Memento.Instance.FullName);
            //Getting FullName property from the memento doesn't trigget more properties acceseses to the original model
            Assert.AreEqual(1, Mod.FullNameExecutionCount);



            //2 properties should be coppied back to the original class, this are all non virtual properties that accept writing
            Assert.AreEqual(2, Memento.ModifiedProperties.Count());

            //Change a property on the memento:
            Memento.Instance.FirstName = "Alejandra";

            //The original class hasn't been changed
            Assert.AreEqual(Mod.FirstName, "Rafael");
        }
    }
}
