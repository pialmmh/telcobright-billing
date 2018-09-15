using System;
using System.Collections.Generic;
using LibraryExtensions;
using MySql.Data.MySqlClient;
namespace TelcobrightMediation
{


    public class TupleByPeriod : IEquatable<TupleByPeriod>
    {
        public int? IdAssignmentTuple { get; set; }
        public DateRange DRange { get; set; }
        public int Priority { get; set; }

        public override string ToString()
        {
            return this.IdAssignmentTuple.ToString() + "/" + this.DRange.ToString();
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 29 + this.DRange.GetHashCode();
                hash = hash * 29 + this.IdAssignmentTuple.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TupleByPeriod);
        }

        public bool Equals(TupleByPeriod obj)
        {
            return obj != null &&
                   (
                       obj.IdAssignmentTuple == this.IdAssignmentTuple &&
                       obj.DRange.Equals(this.DRange));
        }

        public class EqualityComparer : IEqualityComparer<TupleByPeriod> //for dictionary key lookup
        {
            public bool Equals(TupleByPeriod x, TupleByPeriod y)
            {
                return x != null
                       && y != null
                       && (x.DRange.Equals(y.DRange) && x.IdAssignmentTuple == y.IdAssignmentTuple);
            }
            public int GetHashCode(TupleByPeriod x)
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 29 + x.DRange.GetHashCode();
                    hash = hash * 29 + x.IdAssignmentTuple.GetHashCode();
                    return hash;
                }
            }
        }
    }

    public class RateTuple: IEquatable<RateTuple>
    {
        public int? IdRateplanAssignmenttuple = -1;
        public long? IdService = null;
        public DateRange DRange = new DateRange();
        public int? IdRatePlan = null;
        public ServiceAssignmentDirection AssignDirection = ServiceAssignmentDirection.None;
        public int? IdPartner = null;
        public int? IdRoute = null;
        public int? Priority = 1;

        public override string ToString()
        {
            return "id=" + this.IdRateplanAssignmenttuple.ToString() + "/"
                + "srv=" + this.IdService + "/" +
                "prt=" + this.IdPartner == null ? "" : this.IdPartner.ToString() + "/" +
                "ad=" + this.AssignDirection + "/" +
                "priority=" + this.Priority == null ? "" : this.Priority.ToString() + "/" +
                "daterange=" + this.DRange.ToString() + "/" +
                "rp=" + this.IdRatePlan == null ? "" : this.IdRatePlan.ToString();
        }

        public RateTuple(
                        long? pIdService,
                        DateRange pDRange,
                        int? pIdRatePlan,
                        ServiceAssignmentDirection pAssignDirection,
                        int? pIdPartner,
                        int? pIdRoute,
                        int? pPriority
                        )
        {
            this.IdService = pIdService;
            this.DRange = pDRange;
            this.IdRatePlan = pIdRatePlan;
            this.AssignDirection = pAssignDirection;
            this.IdPartner = pIdPartner;
            this.IdRoute = pIdRoute;
            this.Priority = pPriority;
        }
        

        public override int GetHashCode()
        {
            return
                (this.IdService == null ? 0 : this.IdService.GetHashCode()) ^
                (this.DRange == null ? 0 : this.DRange.GetHashCode()) ^
                (this.IdRatePlan == null ? 0 : this.IdRatePlan.GetHashCode()) ^
                (this.AssignDirection == null ? 0 : this.AssignDirection.GetHashCode()) ^
                (this.IdPartner == null ? 0 : this.IdPartner.GetHashCode()) ^
                (this.IdRoute == null ? 0 : this.IdRoute.GetHashCode()) ^
                (this.Priority == null ? 0 : this.Priority.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RateTuple);
        }
        public bool Equals(RateTuple obj)
        {
            return obj != null &&
                (
                obj.IdService == this.IdService &&
                obj.DRange == this.DRange &&
                obj.IdRatePlan == this.IdRatePlan &&
                obj.AssignDirection == this.AssignDirection &&
                obj.IdPartner == this.IdPartner &&
                obj.IdRoute == this.IdRoute &&
                obj.Priority == this.Priority);
        }
        public RateTuple Copy()
        {
            return new RateTuple(this.IdService, this.DRange, this.IdRatePlan, this.AssignDirection, this.IdPartner, this.IdRoute, this.Priority);
        }
    }
}