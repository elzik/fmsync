using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using Thinktecture.IO;
using Polly.Registry;

namespace Elzik.FmSync.Application
{
    public class ResilientFrontMatterFileSynchroniser : IResilientFrontMatterFileSynchroniser
    {
        private readonly ResiliencePipeline _fileWriteResiliencePipeline;
        private readonly IFrontMatterFileSynchroniser _frontMatterFileSynchroniser;

        public ResilientFrontMatterFileSynchroniser(IFrontMatterFileSynchroniser frontMatterFileSynchroniser,
                                                    ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            _fileWriteResiliencePipeline = resiliencePipelineProvider.GetPipeline(Retry5TimesPipelineBuilder.StrategyName);
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
