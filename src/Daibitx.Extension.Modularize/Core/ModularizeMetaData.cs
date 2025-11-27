using System.Reflection;

namespace Daibitx.Extension.Modularize.Model
{
    public class ModularizeMetaData
    {
        /// <summary>
        /// Configuration file name
        /// </summary>
        public string ModuleSettingName { get; set; }
        /// <summary>
        /// Module name
        /// </summary>
        public string ModuleName { get; set; }
        /// <summary>
        ///  folder name of module dictionary
        /// </summary>
        public string ModuleDictionaryName { get; set; }
        /// <summary>
        /// load module base information
        /// </summary>
        public ModularizeMetaData()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            ModuleName = assembly.GetName().Name + ".dll";

            ModuleSettingName = assembly.GetName().Name + ".json";

            ModuleDictionaryName = assembly.GetName().Name;
        }

        public ModularizeMetaData(Assembly assembly)
        {
            ModuleName = assembly.GetName().Name + ".dll";

            ModuleSettingName = assembly.GetName().Name + ".json";

            ModuleDictionaryName = assembly.GetName().Name;
        }
    }
}
