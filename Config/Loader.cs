using System;
using System.IO;
using System.Collections.Generic;

namespace FlexConfirmMail
{
    public class Loader
    {
        public static Config LoadFromDir(string basedir)
        {
            Config config = new Config();
            Dictionary<string, string> dict;
            List<string> list;

            dict = FileConfig.ReadDict(Path.Combine(basedir, "Common.txt"));
            if (dict != null)
            {
                foreach (KeyValuePair<string, string> kv in dict)
                {
                    if (Assign(config, kv.Key, kv.Value))
                    {
                        QueueLogger.Log($"* Option: {kv.Key} = {kv.Value}");
                    }
                    else
                    {
                        QueueLogger.Log($"* Unknown option: {kv.Key} = {kv.Value}");
                    }
                }
            }

            list = FileConfig.ReadList(Path.Combine(basedir, "TrustedDomains.txt"));
            if (list != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", list));
                config.TrustedDomains.AddRange(list);
                config.Modified.Add(ConfigOption.TrustedDomains);
            }

            list = FileConfig.ReadList(Path.Combine(basedir, "UnsafeDomains.txt"));
            if (list != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", list));
                config.UnsafeDomains.AddRange(list);
                config.Modified.Add(ConfigOption.UnsafeDomains);
            }

            list = FileConfig.ReadList(Path.Combine(basedir, "UnsafeFiles.txt"));
            if (list != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", list));
                config.UnsafeFiles.AddRange(list);
                config.Modified.Add(ConfigOption.UnsafeFiles);
            }
            return config;
        }

        public static Config LoadFromReg(string basedir)
        {
            Config config = new Config();
            Dictionary<string, string> dict;


            dict = RegistryConfig.ReadDict(basedir);
            if (dict != null)
            {
                foreach (KeyValuePair<string, string> kv in dict)
                {
                    if (Assign(config, kv.Key, kv.Value))
                    {
                        QueueLogger.Log($"* Option: {kv.Key} = {kv.Value}");
                    }
                    else
                    {
                        QueueLogger.Log($"* Unknown option: {kv.Key} = {kv.Value}");
                    }
                }
            }

            dict = RegistryConfig.ReadDict(basedir + "\\TrustedDomains");
            if (dict != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", dict.Values));
                config.TrustedDomains.AddRange(dict.Values);
                config.Modified.Add(ConfigOption.TrustedDomains);
            }

            dict = RegistryConfig.ReadDict(basedir + "\\UnsafeDomains");
            if (dict != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", dict.Values));
                config.UnsafeDomains.AddRange(dict.Values);
                config.Modified.Add(ConfigOption.UnsafeDomains);
            }

            dict = RegistryConfig.ReadDict(basedir + "\\UnsafeFiles");
            if (dict != null)
            {
                QueueLogger.Log("* List: " + String.Join(" ", dict.Values));
                config.UnsafeFiles.AddRange(dict.Values);
                config.Modified.Add(ConfigOption.UnsafeFiles);
            }
            return config;
        }

        private static bool Assign(Config config, string key, string val)
        {
            int i;
            bool b;

            if (key == "CountEnabled")
            {
                if (ParseBool(val, out b))
                {
                    config.CountEnabled = b;
                    config.Modified.Add(ConfigOption.CountEnabled);
                    return true;
                }
            }

            if (key == "CountSeconds")
            {
                if (ParseInt(val, out i))
                {
                    config.CountSeconds = i;
                    config.Modified.Add(ConfigOption.CountSeconds);
                    return true;
                }
            }

            if (key == "CountAllowSkip")
            {
                if (ParseBool(val, out b))
                {
                    config.CountAllowSkip = b;
                    config.Modified.Add(ConfigOption.CountAllowSkip);
                    return true;
                }
            }

            if (key == "SafeBccEnabled")
            {
                if (ParseBool(val, out b))
                {
                    config.SafeBccEnabled = b;
                    config.Modified.Add(ConfigOption.SafeBccEnabled);
                    return true;
                }
            }

            if (key == "SafeBccThreshold")
            {
                if (ParseInt(val, out i))
                {
                    config.SafeBccThreshold = i;
                    config.Modified.Add(ConfigOption.SafeBccThreshold);
                    return true;
                }
            }

            if (key == "MainSkipIfNoExt")
            {
                if (ParseBool(val, out b))
                {
                    config.MainSkipIfNoExt = b;
                    config.Modified.Add(ConfigOption.MainSkipIfNoExt);
                    return true;
                }
            }
            return false;
        }

        private static bool ParseBool(string val, out bool ret)
        {
            val = val.ToLower();
            if (val == "yes" || val == "true" || val == "on" || val == "1")
            {
                ret = true;
                return true;
            }

            if (val == "no" || val == "false" || val == "off" || val == "0")
            {
                ret = false;
                return true;
            }

            ret = false;
            return false;
        }

        private static bool ParseInt(string val, out int ret)
        {
            try
            {
                ret = int.Parse(val);
                return true;
            }
            catch
            {
                ret = -1;
                return false;
            }
        }
    }
}