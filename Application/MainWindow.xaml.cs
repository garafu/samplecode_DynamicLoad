using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;

namespace DynamicLoad
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 実行ボタンを押下したとき呼び出されます。
        /// </summary>
        /// <param name="sender">イベント発生元</param>
        /// <param name="e">イベントオブジェクト</param>
        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            var interfaceName = typeof(IPlugin).FullName;
            var typeName = string.Empty;
            IPlugin plugin = null;

            // プラグインファイルパス一覧の取得
            var pluginFilePathList = this.GetPluginFilePathList();

            // プラグインからインスタンス生成
            foreach (var pluginFilePath in pluginFilePathList)
            {
                plugin = this.CreateInstance<IPlugin>(pluginFilePath);
                if (plugin == null)
                {
                    continue;
                }
                this.textBox_filename.Text += plugin.Message();
            }
        }

        /// <summary>
        /// プラグインファイルパス一覧を取得します。
        /// </summary>
        /// <returns>プラグインへのファイルパス一覧</returns>
        private string[] GetPluginFilePathList()
        {
            var entryDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var pluginDirectory = @"plugin";
            var targetDirectory = System.IO.Path.Combine(entryDirectory, pluginDirectory);

            if (!Directory.Exists(targetDirectory))
            {
                return new string[] { };
            }

            return Directory.GetFiles(targetDirectory, "*.dll", SearchOption.AllDirectories);
        }

        /// <summary>
        /// 指定された dll のインスタンスを生成します。
        /// </summary>
        /// <typeparam name="T">インターフェース クラス</typeparam>
        /// <param name="filepath">プラグイン ファイルパス</param>
        /// <returns>プラグイン から生成 されたインスタンス</returns>
        private T CreateInstance<T>(string filepath)
            where T : class
        {
            Assembly assembly = null;
            string typeName = string.Empty;
            string interfaceName = typeof(T).FullName;

            assembly = Assembly.LoadFrom(filepath);

            if (assembly == null)
            {
                return null;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && type.IsPublic && !type.IsAbstract && type.GetInterface(interfaceName) != null)
                {
                    typeName = type.FullName;
                    break;
                }
            }

            return assembly.CreateInstance(typeName) as T;
        }
    }
}
