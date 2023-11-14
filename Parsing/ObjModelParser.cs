using ObjRenderer.Models;

namespace ObjRenderer.Parsing
{
    public class ObjModelParser
    {
        private const string ObjFilePath = "/Model.obj";
        private const string SpecularMapPath = "/textures/specular.png";
        private const string NormalMapPath = "/textures/normal.png";
        private const string DiffuseMapPath = "/textures/texture.png";

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

            model.normalMap = MapFileParser.LoadMap(modelDirectoryPath + NormalMapPath);
            model.specularMap = MapFileParser.LoadMap(modelDirectoryPath + SpecularMapPath);
            model.diffuseMap = MapFileParser.LoadMap(modelDirectoryPath + DiffuseMapPath);

            return model;
        }

    }
}
