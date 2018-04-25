using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Spring.Expressions.Parser.antlr;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Mediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;

namespace TelcobrightMediation.Cdr
{
    public enum CdrNewOldType
    {
        NewCdr,
        OldCdr
    }

    public class CdrExt
    {
        private PartialCdrContainer _partialCdrContainer;
        private readonly List<acc_transaction> _reversedTransactions = new List<acc_transaction>();
        public CdrNewOldType CdrNewOldType { get; }
        public string UniqueBillId => this.Cdr.UniqueBillId;
        public long IdCall => this.Cdr.idcall;
        public DateTime StartTime => this.Cdr.StartTime;
        public cdr Cdr { get; set; }
        public Dictionary<string, AbstractCdrSummary> TableWiseSummaries { get; set; }

        public Dictionary<ValueTuple<int, int, int>, acc_chargeable> Chargeables { get; }
            = new Dictionary<ValueTuple<int, int, int>, acc_chargeable>(); //key=tuple(sg,sf,assignedDir)

        public Dictionary<long, TransactionContainerForSingleAccount> AccWiseTransactionContainers { get; } =
            new Dictionary<long, TransactionContainerForSingleAccount>();

        public CdrMediationResult MediationResult { get; set; }

        public PartialCdrContainer PartialCdrContainer
        {
            get
            {
                if (this.CdrNewOldType == CdrNewOldType.OldCdr)//old cdr
                {
                    throw new Exception("Property PartialCdrContainer cannot be used for old cdrs.");
                }
                else if (this.Cdr.PartialFlag == 0)//new non partial cdr
                {
                    throw new Exception("Property PartialCdrContainer cannot be used for non partial cdrs.");
                }
                return this._partialCdrContainer;
            }
            set
            {
                if (this.CdrNewOldType == CdrNewOldType.OldCdr && value != null)//old cdr
                {
                    throw new Exception("Property PartialCdrContainer cannot be set for old cdrs.");
                }
                else if (this.Cdr.PartialFlag == 0 && value != null)//new non partial cdr
                {
                    throw new Exception("Property PartialCdrContainer cannot be set for non partial cdrs.");
                }
                this._partialCdrContainer = value;
            }
        }

        public CdrExt(cdr cdr, CdrNewOldType cdrExtType)
        {
            this.Cdr = cdr;
            this.CdrNewOldType = cdrExtType;

            this.TableWiseSummaries = new Dictionary<string, AbstractCdrSummary>();
        }

        public override string ToString()
        {
            return this.UniqueBillId + "/" + this.Cdr.idcall;
        }
    }
}
