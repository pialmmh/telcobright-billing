using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public interface IDataReaderToStrArrConvertable
    {
        string[] ConvertDataReaderToStrArr(object inputData, IDataReader dataReader);
    }

}
