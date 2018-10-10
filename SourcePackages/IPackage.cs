using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourcePackages
{
    public enum PackageType

    {

        Nuget,

        Npm

    }
    public interface IPackageMagic
    {
        string Name { get; set; }
        string Version { get; set; }

        string UniqueName { get; set; }
        PackageType PackageType { get; set; }
    }

    public static class FactoryPackages
    {
        static List<IPackageMagic> packages;
        static FactoryPackages()
        {
            packages = new List<IPackageMagic>();
        }

        public static List<IPackageMagic> GetPackages()
        {
            return packages;
        }

        public static void AddPackage(IPackageMagic package)
        {
            IPackageMagic checkIfExist = packages.Find(z => z.Name == package.Name && z.Version == package.Version);
            if (checkIfExist == null)
            {
                packages.Add(package);

            }
            else
            {
                //Console.WriteLine("Skipped - " + package.Name + " " + package.Version);
            }

        }
        public static void RemovePackage(IPackageMagic package)
        {
            packages.Remove(package);
        }

    }



    //public class Npm : IPackage
    //{
    //    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //}
}
