using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSerializer = System.Xml.Serialization.XmlSerializer;

namespace ClangPowerTools
{
  public class XmlSerializer
  {
    public void SerializeToFile(string aFilePath, object obj)
    {
      NetSerializer xmlSerializer = new NetSerializer(obj.GetType());

      using (FileStream fs = new FileStream(aFilePath, FileMode.Create))
      {
        using (StreamWriter sw = new StreamWriter(fs))
        {
          xmlSerializer.Serialize(sw, obj);
        }
      }
    }

    public T DeserializeFromFIle<T>(string aFilePath)
    {
      NetSerializer serializer = new NetSerializer(typeof(T));
      using (FileStream fs = new FileStream(aFilePath, FileMode.Open))
      {
        return (T)serializer.Deserialize(fs);
      }
    }

  }
}
