using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using _4thIR.TextGenerateT.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using TextGeneration.Transformers;

namespace _4thIR.TextGenerateT.Activities
{
    [LocalizedDisplayName(nameof(Resources.TextGeneration_DisplayName))]
    [LocalizedDescription(nameof(Resources.TextGeneration_Description))]
    public class TextGeneration : ContinuableAsyncCodeActivity
    {

        private static readonly TextGeneratorTransformers generator=new TextGeneratorTransformers();

        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.Timeout_DisplayName))]
        [LocalizedDescription(nameof(Resources.Timeout_Description))]
        public InArgument<int> TimeoutMS { get; set; } = 60000;

        [LocalizedDisplayName(nameof(Resources.TextGeneration_Sentence_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextGeneration_Sentence_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Sentence { get; set; }

        [LocalizedDisplayName(nameof(Resources.TextGeneration_GeneratedText1_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextGeneration_GeneratedText1_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> GeneratedText1 { get; set; }

        [LocalizedDisplayName(nameof(Resources.TextGeneration_GeneratedText2_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextGeneration_GeneratedText2_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> GeneratedText2 { get; set; }

        #endregion


        #region Constructors

        public TextGeneration()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Sentence == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Sentence)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var timeout = TimeoutMS.Get(context);
            

            // Set a timeout on the execution
            var task = ExecuteWithTimeout(context, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) != task) throw new TimeoutException(Resources.Timeout_Error);

            // Outputs
            return (ctx) => {
                GeneratedText1.Set(ctx, task.Result.Item1);
                GeneratedText2.Set(ctx, task.Result.Item2);
            };
        }

        private async Task<Tuple<string, string>> ExecuteWithTimeout(AsyncCodeActivityContext context, CancellationToken cancellationToken = default)
        {
            var sentence = Sentence.Get(context);

            var res=await generator.GenerateText(sentence);

            return res;
        }

        #endregion
    }
}

