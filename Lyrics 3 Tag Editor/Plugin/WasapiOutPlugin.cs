using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace Lyrics_3_Tag_Editor.Plugin
{
    public class WasapiOutPlugin : IOutputDevicePlugin
    {
        private bool allowShareMode = false;
        private bool allowEventCallback = false;

        private MMDeviceEnumerator device_enumerator = null;
        private MMDeviceCollection device_collection = null;
        private int device_index = -1;

        public IWavePlayer Create(int latency)
        {
            device_enumerator = new MMDeviceEnumerator();
            device_collection = device_enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (MMDevice device in device_collection)
            {
                device_index++;
                if (device.FriendlyName.Equals(device_enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ToString()))
                    break;
            }

            WasapiOut wasapi = new WasapiOut(
                SelectedDevice,
                ShareMode,
                UseEventCallback,
                latency);

            return wasapi;
        }

        public MMDevice SelectedDevice
        {
            get
            {
                return device_collection[device_index];
            }
        }

        public AudioClientShareMode ShareMode
        {
            get
            {
                return allowShareMode ? 
                    AudioClientShareMode.Exclusive : 
                    AudioClientShareMode.Shared;
            }
        }

        public bool UseEventCallback
        {
            get
            {
                return allowEventCallback;
            }
        }
    }
}
