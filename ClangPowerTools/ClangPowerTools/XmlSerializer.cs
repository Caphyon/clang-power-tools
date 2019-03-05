using System.IO;
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

    public T DeserializeFromFile<T>(string aFilePath)
    {
      NetSerializer serializer = new NetSerializer(typeof(T));
      using (FileStream fs = new FileStream(aFilePath, FileMode.Open))
      {
        return (T)serializer.Deserialize(fs);
      }
    }

  }
}
