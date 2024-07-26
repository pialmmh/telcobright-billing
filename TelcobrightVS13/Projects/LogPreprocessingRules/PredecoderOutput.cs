using LibraryExtensions;
using MediationModel;

namespace LogPreProcessor
{
    class PredecoderOutput
    {
        public job SuccessfulJob { get; set; }
        public long WrittenFileSize { get; set; }
        public job FailedJob { get; set; }
        public string ExceptionMessage { get; set; }
        public DateRange DateRange { get; set; }
    }
}