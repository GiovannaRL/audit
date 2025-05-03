using xPlannerCommon.Models;

namespace OfflineXPlanner.Domain
{
    public class JSN
    {
        public int ID { get; set; }
        public string JSNCode { get; set; }
        public string JSNNomenclature { get; set; }
        public string U1 { get; set; }
        public string U2 { get; set; }
        public string U3 { get; set; }
        public string U4 { get; set; }
        public string U5 { get; set; }
        public string U6 { get; set; }

        public jsn toCommonModel()
        {
            return new jsn() {
                Id = this.ID,
                nomenclature = this.JSNNomenclature,
                jsn_code = this.JSNCode,
                utility1 = this.U1,
                utility2 = this.U2,
                utility3 = this.U3,
                utility4 = this.U4,
                utility5 = this.U5,
                utility6 = this.U6
            };
        }

        public override bool Equals(object obj)
        {
            JSN jsn = obj as JSN;
             
            if (jsn == null || !JSNCode.Equals(jsn.JSNCode) || !JSNNomenclature.Equals(jsn.JSNNomenclature))
            {
                return false;
            }

            return U1?.ToLower() != jsn.U1?.ToLower()
                || U2?.ToLower() != U2?.ToLower()
                || U3?.ToLower() != U3?.ToLower()
                || U4?.ToLower() != U4?.ToLower()
                || U5?.ToLower() != U5?.ToLower()
                || U6?.ToLower() != U6?.ToLower();
        }

        public override int GetHashCode()
        {
            return JSNCode.GetHashCode()
                    + JSNNomenclature.GetHashCode()
                    + (U1?.GetHashCode() ?? 0)
                    + (U2?.GetHashCode() ?? 0)
                    + (U3?.GetHashCode() ?? 0)
                    + (U4?.GetHashCode() ?? 0)
                    + (U5?.GetHashCode() ?? 0)
                    + (U6?.GetHashCode() ?? 0);
        }
    }
}
