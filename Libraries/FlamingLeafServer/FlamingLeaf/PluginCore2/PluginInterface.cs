using System;

namespace PluginInterface
{
	public interface IFlamingPlugin
	{
		IFlamingPluginHost Host {get;set;}
		
		string Name {get;}
		string Description {get;}
		string Author {get;}
		string Version {get;}
		
		object HostObject {get;set;}
		
        int InvokePlugin(FlamingLeaf.Api.FlamingApiRequest apiRequest, FlamingLeafToolkit.FlamingApiClientInformation apiClientInformation);

        void LoadPlugin();
        void Initialize();
		void Dispose();
	
	}
	
	public interface IFlamingPluginHost
	{
		//void Feedback(string Feedback, IPlugin Plugin);	
	}
}
