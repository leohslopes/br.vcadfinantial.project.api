
namespace br.vcadfinantial.project.domain.Exceptions
{
    public class FileAlreadyExistsException : Exception
    {
        public string MounthKey { get; }
        public string ExistingFileName { get; }

        public FileAlreadyExistsException(string mounthKey, string existingFileName)
            : base($"Já existe um arquivo vigente para o código base {mounthKey}. \n Arquivo recebido: {existingFileName}")
        {
            MounthKey = mounthKey;
            ExistingFileName = existingFileName;
        }
    }
}
