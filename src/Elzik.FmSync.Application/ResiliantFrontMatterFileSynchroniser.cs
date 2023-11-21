using Elzik.FmSync.Domain;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using Thinktecture.IO;
using Polly.Registry;

namespace Elzik.FmSync
{
    public class ResiliantFrontMatterFileSynchroniser : FrontMatterFileSynchroniser
    {
        private readonly ResiliencePipeline fileWriteResiliencePipeline;

        public ResiliantFrontMatterFileSynchroniser(ILogger<FrontMatterFileSynchroniser> logger,
                                                    IMarkdownFrontMatter markdownFrontMatter,
                                                    IFile file,
                                                    ResiliencePipelineProvider<string> resiliencePipelineProvider)
            : base(logger, markdownFrontMatter, file)
        {
            fileWriteResiliencePipeline = resiliencePipelineProvider.GetPipeline("retry-5-times");
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
