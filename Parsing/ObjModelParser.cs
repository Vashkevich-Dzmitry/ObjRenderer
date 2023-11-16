using ObjRenderer.Models;

namespace ObjRenderer.Parsing
{
    public class ObjModelParser
    {
        private const string ObjFilePath = "/Model.obj";
        private const string SpecularMapPath = "/textures/specular.png";
        private const string NormalMapPath = "/textures/normal.png";
        private const string DiffuseMapPath = "/textures/diffuse.png";

        private string modelDirectoryPath;
        public string ModelDirectoryPath
        {
            get => modelDirectoryPath;
            set => modelDirectoryPath = value;
        }

        public ObjModelParser(string modelDirectoryPath)
        {
            this.modelDirectoryPath = modelDirectoryPath;
        }

        public Model ParseModel()
        {
            Model model = ObjFileParser.ParseObj(modelDirectoryPath + ObjFilePath);

            model.NormalMap = MapFileParser.LoadNormalMap(modelDirectoryPath + NormalMapPath);
            model.SpecularMap = MapFileParser.LoadSpecularMap(modelDirectoryPath + SpecularMapPath);
            model.DiffuseMap = MapFileParser.LoadDiffuseMap(modelDirectoryPath + DiffuseMapPath);

            return model;
        }

    }
}
