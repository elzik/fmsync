using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using Thinktecture.IO;
using Polly.Registry;

namespace Elzik.FmSync
{
    public class ResiliantFrontMatterFileSynchroniser : IResiliantFrontMatterFileSynchroniser
    {
        private readonly ResiliencePipeline _fileWriteResiliencePipeline;
        private readonly IFrontMatterFileSynchroniser _frontMatterFileSynchroniser;

        public ResiliantFrontMatterFileSynchroniser(IFrontMatterFileSynchroniser frontMatterFileSynchroniser,
                                                    ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            _fileWriteResiliencePipeline = resiliencePipelineProvider.GetPipeline("retry-5-times");
            _frontMatterFileSynchroniser = frontMatterFileSynchroniser;
        }

        public SyncResult SyncCreationDate(string markDownFilePath)
        {
            return _fileWriteResiliencePipeline.Execute(() =>
            {
                return _frontMatterFileSynchroniser.SyncCreationDate(markDownFilePath);
            });
        }
    }
}
