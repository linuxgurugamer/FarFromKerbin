using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarFromKerbin
{
	internal class ModuleSTPModularCorridor : PartModule
	{
		public class ModeConfig
		{
			public string path;

			public ConfigNode config;

			public ConfigNode[] moduleconfs;

			public ConfigNode[] resconfs;

			public ConfigNode[] inRes;

			public ConfigNode[] modFilters;

			public bool isDepsOk = true;

			private bool _isInit = false;

			public ModeConfig(string path, ConfigNode config)
			{
				this.path = path;
				this.config = config;
                
				this.moduleconfs = config.GetNodes("MODULE");
				this.resconfs = config.GetNodes("RESOURCE");
				this.inRes = config.GetNodes(ModuleSTPModularCorridor.inResNodeName);
				this.modFilters = config.GetNodes(ModuleSTPModularCorridor.modFilterNodeName);
				if (this.modFilters.Length != 0)
				{
					ConfigNode[] array = this.modFilters;
					for (int j = 0; j < array.Length; j++)
					{
						ConfigNode i = array[j];
						bool isOk = false;
						foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
						{
							if (i.GetValue("name") == a.name)
							{
								isOk = true;
							}
						}
						if (!isOk)
						{
							this.isDepsOk = false;
							Debug.LogError("[STB]: Dependencies of " + config.GetValue("modeName") + " cannot found");
							break;
						}
					}
				}
				this._isInit = true;
			}

			public ModeConfig()
			{
			}

			public void Load(ConfigNode node)
			{
				if (node.HasValue("modeConfigPath"))
				{
					this.path = node.GetValue("modeConfigPath");
					this.config = GameDatabase.Instance.GetConfigNode(this.path + "/STBMODE");
					this.moduleconfs = this.config.GetNodes("MODULE");
					this.resconfs = this.config.GetNodes("RESOURCE");
					this.inRes = this.config.GetNodes(ModuleSTPModularCorridor.inResNodeName);
					this.modFilters = this.config.GetNodes(ModuleSTPModularCorridor.modFilterNodeName);
					Debug.Log(this.path + "/STBMODE is loaded! name: " + this.config.name);
					System.Collections.Generic.List<ConfigNode> moduleList = new System.Collections.Generic.List<ConfigNode>();
					ConfigNode[] nodes = node.GetNodes("MODULE");
					for (int j = 0; j < nodes.Length; j++)
					{
						ConfigNode n = nodes[j];
						ConfigNode[] array = this.moduleconfs;
						for (int k = 0; k < array.Length; k++)
						{
							ConfigNode n2 = array[k];
							if (n.GetValue("name") == n2.GetValue("name"))
							{
								moduleList.Add(n);
								Debug.Log("[STB] says: Loading module " + n.GetValue("name"));
							}
						}
					}
					if (moduleList.Count != 0)
					{
						this.moduleconfs = moduleList.ToArray();
					}
					if (this.modFilters.Length != 0)
					{
						nodes = this.modFilters;
						for (int j = 0; j < nodes.Length; j++)
						{
							ConfigNode i = nodes[j];
							bool isOk = false;
							foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
							{
								if (i.GetValue("name") == a.name)
								{
									isOk = true;
								}
							}
							if (!isOk)
							{
								this.isDepsOk = false;
								break;
							}
						}
					}
					this._isInit = true;
				}
				else
				{
					Debug.Log("[STB] says: Cannot load mode!");
				}
			}

			public void Save(ConfigNode node, Part p)
			{
				node.SetValue("modeConfigPath", this.path, true);
				Debug.Log("[STB] says: Saving modules...");
				foreach (PartModule pm in p.Modules)
				{
					ConfigNode[] array = this.moduleconfs;
					for (int j = 0; j < array.Length; j++)
					{
						ConfigNode i = array[j];
						if (pm.moduleName == i.GetValue("name"))
						{
							pm.Save(node);
							Debug.Log("[STB] says: Saving " + pm.moduleName);
						}
					}
				}
			}

			public override string ToString()
			{
				return this.path;
			}

			public bool hasInit()
			{
				return this._isInit;
			}
		}

		private bool showInstallGui = false;

		private Rect installWindow = new Rect(0f, 0f, 200f, 0f);

		private Vector2 scrollPosition;

		private System.Collections.Generic.List<ModuleSTPModularCorridor.ModeConfig> modes = new System.Collections.Generic.List<ModuleSTPModularCorridor.ModeConfig>();

		public static string inResNodeName = "RESOURCE-COST";

		public static string modFilterNodeName = "MOD-DEPENDENCY";

		public ModuleSTPModularCorridor.ModeConfig installedModule = new ModuleSTPModularCorridor.ModeConfig();

		[KSPField(guiActive = true, guiName = "Current mode")]
		public string currModeName = "None";

		private GameObject currModel;

		public System.Collections.Generic.List<string> hiddenPanels = new System.Collections.Generic.List<string>();

		[KSPEvent(guiName = "Build in!", guiActiveUnfocused = true, unfocusedRange = 10f, externalToEVAOnly = true)]
		public void BuildIn()
		{
			this.CheckPanels();
		}

		[KSPEvent(guiName = "Install mode!", guiActiveUnfocused = true, unfocusedRange = 10f, externalToEVAOnly = true)]
		public void InstallMode()
		{
			this.showInstallGui = true;
		}

		public override void OnAwake()
		{
			base.OnAwake();
			foreach (UrlDir.UrlFile f in GameDatabase.Instance.root.AllConfigFiles)
			{
				foreach (UrlDir.UrlConfig c in f.configs)
				{
					if (c.config.name == "STBMODE")
					{
						this.modes.Add(new ModuleSTPModularCorridor.ModeConfig(f.url, c.config));
						Debug.Log("[STB]: Adding " + c.config.GetValue("modeName"));
					}
				}
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			Debug.Log("[STB]: Start loading...");
			if (!this.installedModule.hasInit())
			{
				Debug.Log("[STB]: Start loading last state...");
				this.installedModule.Load(node);
				if (this.installedModule.hasInit())
				{
					this.InstallMode(this.installedModule, true);
					Debug.Log("[STB]: Loaded installed module: " + this.installedModule.config.name);
				}
			}
			ConfigNode[] nodes = node.GetNodes("HIDDEN_NODE");
			for (int j = 0; j < nodes.Length; j++)
			{
				ConfigNode i = nodes[j];
				this.hiddenPanels.Add(i.GetValue("name"));
			}
			Debug.Log("[STB]: End loading...");
			this.installWindow.x = (float)(Screen.width / 2);
			this.installWindow.y = (float)(Screen.height / 2);
		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			if (this.installedModule.hasInit())
			{
				this.installedModule.Save(node, base.part);
			}
			if (this.hiddenPanels.Count != 0)
			{
				foreach (string s in this.hiddenPanels)
				{
					ConfigNode i = new ConfigNode("HIDDEN_NODE");
					i.AddValue("name", s);
					node.SetNode(i.name, i, true);
				}
			}
		}

		public void OnGUI()
		{
			GUI.skin = HighLogic.Skin;
			if (this.showInstallGui)
			{
				this.installWindow = GUILayout.Window(base.GetInstanceID(), this.installWindow, new GUI.WindowFunction(this.drawWindow), "Select a mode!", new GUILayoutOption[0]);
			}
		}

		private void drawWindow(int id)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			if (GUI.Button(new Rect(this.installWindow.width - 25f, 5f, 20f, 20f), "X"))
			{
				this.showInstallGui = false;
			}
			GUILayout.EndHorizontal();
			this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, new GUILayoutOption[]
			{
				GUILayout.Width(175f),
				GUILayout.Height(150f)
			});
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			foreach (ModuleSTPModularCorridor.ModeConfig i in this.modes)
			{
				if (i.path != this.installedModule.path)
				{
					if (GUILayout.Button(i.config.GetValue("modeName"), new GUILayoutOption[]
					{
						GUILayout.Width(150f)
					}))
					{
					}
					this.InstallMode(i, false);
					this.showInstallGui = false;
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUI.DragWindow();
		}

		private void InstallMode(ModuleSTPModularCorridor.ModeConfig m, bool isLoading = false)
		{
			ConfigNode conf = m.config;
			int idx = m.path.LastIndexOf('/');
			string modelPath = m.path.Substring(0, idx) + "/" + conf.GetValue("model");
			Debug.Log("[STB]: Loading: " + conf.GetValue("modeName") + " mode.");
			ConfigNode[] array = m.inRes;
			for (int j = 0; j < array.Length; j++)
			{
				ConfigNode i = array[j];
				if (MBToolbox.AmountAvailable(i.GetValue("name"), FlightGlobals.ActiveVessel.rootPart) < System.Convert.ToDouble(i.GetValue("amount")))
				{
					ScreenMessages.PostScreenMessage("Not enough " + i.GetValue("name"), 5f);
					return;
				}
				MBToolbox.RequestResource(i.GetValue("name"), System.Convert.ToDouble(i.GetValue("amount")), FlightGlobals.ActiveVessel.rootPart);
			}
			if (this.installedModule.hasInit() && !isLoading)
			{
				Debug.Log("[STB]: Reseting modules...");
				if (this.installedModule.moduleconfs.Length != 0)
				{
					System.Collections.Generic.List<PartModule> pmToRemoveList = new System.Collections.Generic.List<PartModule>();
					using (var enumerator = base.part.Modules.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PartModule pm = (PartModule)enumerator.Current;
							array = this.installedModule.moduleconfs;
							for (int j = 0; j < array.Length; j++)
							{
								ConfigNode i = array[j];
								if (i.GetValue("name") == pm.moduleName)
								{
									pmToRemoveList.Add(pm);
								}
							}
						}
					}
					foreach (PartModule pm in pmToRemoveList)
					{
						base.part.RemoveModule(pm);
						Debug.Log("[STB]: Removing: " + pm.moduleName + " module.");
					}
				}
				Debug.Log("[STB]: Reseting resources...");
				foreach (PartResource r in base.part.Resources)
				{
					array = this.installedModule.resconfs;
					for (int j = 0; j < array.Length; j++)
					{
						ConfigNode i = array[j];
						if (i.GetValue("name") == r.resourceName)
						{
							ConfigNode node = i.CreateCopy();
							node.SetValue("name", r.resourceName, false);
							node.SetValue("amount", "0", false);
							node.SetValue("maxAmount", "0", false);
							base.part.SetResource(node);
							Debug.Log("[STB]: Reseting " + r.resourceName);
						}
					}
				}
				Debug.Log("[STB]: Reseting model...");
				if (this.currModel != null)
				{
					this.currModel.SetActive(false);
				}
			}
			if (m.isDepsOk)
			{
				Debug.Log("[STB]: Adding modules...");
				array = m.moduleconfs;
				for (int j = 0; j < array.Length; j++)
				{
					ConfigNode i = array[j];
					PartModule pm = base.part.AddModule(i.GetValue("name"));
					pm.OnAwake();
					pm.OnInitialize();
					pm.Load(i);
					pm.OnStart(PartModule.StartState.Landed);
					Debug.Log(i.GetValue("name") + " is being added from: " + i.name);
				}
			}
			if (!isLoading)
			{
				Debug.Log("[STB]: Adding resources...");
				array = m.resconfs;
				for (int j = 0; j < array.Length; j++)
				{
					ConfigNode i = array[j];
					if (i.GetValue("amount") != "0")
					{
						i.SetValue("amount", "0", false);
					}
					base.part.SetResource(i);
					Debug.Log(i.GetValue("name") + " " + i.name + " is being added");
				}
			}
			Debug.Log("[STB]: Adding model...");
			if (GameDatabase.Instance.ExistsModel(modelPath))
			{
				GameObject go = GameDatabase.Instance.GetModel(modelPath);
				go.SetActive(true);
				go.layer = 15;
				go.name = conf.GetValue("modeName") + go.GetInstanceID();
				go.transform.position = base.part.partTransform.position;
				go.transform.rotation = base.part.partTransform.rotation;
				go.transform.parent = base.part.partTransform;
				this.currModel = go;
			}
			else
			{
				Debug.Log("[STB] says: " + modelPath + " is not found!");
			}
			this.installedModule = m;
			this.currModeName = conf.GetValue("modeName");
			Debug.Log("[STB]: Installing done!");
		}

		public void CheckPanels()
		{
			foreach (AttachNode i in base.part.attachNodes)
			{
				if (!this.hiddenPanels.Contains(i.id))
				{
					if (i.attachedPart != null && i.attachedPart.Modules.Contains(base.GetType().Name))
					{
						Component[] componentsInChildren = base.part.GetComponentsInChildren(typeof(Transform));
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							Transform c = (Transform)componentsInChildren[j];
							if (c.parent != null && c.childCount != 0 && c.GetComponent<Renderer>() != null)
							{
								if (Vector3.Distance(c.GetChild(0).position, base.part.partTransform.TransformPoint(i.position)) <= 0.1f && c.GetChild(0).GetComponent<Collider>() != null)
								{
									c.GetComponent<Renderer>().enabled = false;
									c.GetChild(0).GetComponent<Collider>().enabled = false;
									(i.attachedPart.Modules[base.part.Modules.IndexOf(this)] as ModuleSTPModularCorridor).CheckPanels(base.part);
									this.hiddenPanels.Add(i.id);
								}
							}
						}
					}
				}
			}
		}

		public void CheckPanels(Part otherPart)
		{
			AttachNode i = base.part.FindAttachNodeByPart(otherPart);
			if (!this.hiddenPanels.Contains(i.id))
			{
				Component[] componentsInChildren = base.part.GetComponentsInChildren(typeof(Transform));
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					Transform c = (Transform)componentsInChildren[j];
					if (c.parent != null && c.childCount != 0 && c.GetComponent<Renderer>() != null)
					{
						if (Vector3.Distance(c.GetChild(0).position, base.part.partTransform.TransformPoint(i.position)) <= 0.1f && c.GetChild(0).GetComponent<Collider>() != null)
						{
							c.GetComponent<Renderer>().enabled = false;
							c.GetChild(0).GetComponent<Collider>().enabled = false;
							this.hiddenPanels.Add(i.id);
						}
					}
				}
			}
		}
	}
}
