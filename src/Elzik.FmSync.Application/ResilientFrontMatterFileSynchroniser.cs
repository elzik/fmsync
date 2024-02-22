using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using Thinktecture.IO;
using Polly.Registry;

namespace Elzik.FmSync.Application
{
    public class ResilientFrontMatterFileSynchroniser(
        IFrontMatterFileSynchroniser frontMatterFileSynchroniser,
        ResiliencePipelineProvider<string> resiliencePipelineProvider) : IResilientFrontMatterFileSynchroniser
    {
        private readonly ResiliencePipeline _fileWriteResiliencePipeline 
            = resiliencePipelineProvider.GetPipeline(Retry5TimesPipelineBuilder.StrategyName);
        private readonly IFrontMatterFileSynchroniser _frontMatterFileSynchroniser 
            = frontMatterFileSynchroniser;

        public SyncResult SyncCreationDate(string markDownFilePath)
        {
            return _fileWriteResiliencePipeline.Execute(() =>
            {
                return _frontMatterFileSynchroniser.SyncCreationDate(markDownFilePath);
            });
        }
    }
}
