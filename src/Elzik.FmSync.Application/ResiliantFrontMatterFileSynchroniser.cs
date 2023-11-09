using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using Thinktecture.IO;

namespace Elzik.FmSync
{
    public class ResiliantFrontMatterFileSynchroniser : FrontMatterFileSynchroniser
    {
        private readonly ResiliencePipeline fileWriteResiliencePipeline;

        public ResiliantFrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger, IMarkdownFrontMatter markdownFrontMatter, IFile file) 
            : base(logger, markdownFrontMatter, file)
        {
            fileWriteResiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions()
                {
                    MaxRetryAttempts = 5,
                    BackoffType = DelayBackoffType.Exponential,
                })
                .Build();
        }

        public override SyncResult SyncCreationDate(string markDownFilePath)
        {
            return fileWriteResiliencePipeline.Execute(() =>
            {
                return base.SyncCreationDate(markDownFilePath);
            });
        }
    }
}
