using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HuggingFace.API.Examples {
    public class SpeechRecognition : MonoBehaviour {
        private AudioClip clip;
        private void Start() {
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.R)) {
                StartRecording();
            } else if (Input.GetKeyUp(KeyCode.S)) {
                StopRecording();
            }
        }

        private void StartRecording() {
            Debug.Log("Grabando...");
            clip = Microphone.Start(null, false, 10, 44100);
        }

        private void StopRecording() {
            Debug.Log("Grabación detenida");
            Microphone.End(null);
            byte[] wabData = EncodeAsWAV(clip);
            SendRecording(wabData);
        }

        private void SendRecording(byte[] bytes) {
            HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
                Debug.Log("Texto reconocido: " + response);
                response = response.ToLower();
                ProcessResponse(response);
            }, error => {
                Debug.LogError("Error con la api de HuggingFace: " + error);
            });
        }

        private byte[] EncodeAsWAV(AudioClip clip) {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)clip.channels);
                    writer.Write(clip.frequency);
                    writer.Write(clip.frequency * clip.channels * 2);
                    writer.Write((ushort)(clip.channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples) {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return memoryStream.ToArray();
            }
        }

        private void ProcessResponse(string response) {
            if (response.Contains("jump")) {
                Jump();
            } else if (response.Contains("smaller")) {
                Smaller();
            } else if (response.Contains("bigger")) {
                Bigger();
            }
        }
        private void Jump() {
            Debug.Log("Saltando");
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
        }

        private void Smaller() {
            Debug.Log("Haciendo más pequeño");
            transform.localScale /= 2;
        }

        private void Bigger() {
            Debug.Log("Haciendo más grande");
            transform.localScale *= 2;
        }
    }
}