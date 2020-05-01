﻿// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    using Loading;

    public class GltfAsset : MonoBehaviour
    {
        public string url;
        public bool loadOnStartup = true;

        protected GLTFast gLTFastInstance;

        public UnityAction<GltfAsset,bool> onLoadComplete;

        public string loadingErrorMessage = null;


        /// <summary>
        /// Method for manual loading with custom <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        public void Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, bool? gltfBinary=null ) {
            this.url = url;
            Load(downloadProvider,deferAgent, gltfBinary);
        }

        void Start()
        {
            if(loadOnStartup && !string.IsNullOrEmpty(url)){
                // Automatic load on startup
                Load();
            }
        }

        void Load( IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, bool? gltfBinary=null ) {
            gLTFastInstance = new GLTFast(this,downloadProvider,deferAgent);
            gLTFastInstance.onLoadComplete += OnLoadComplete;
            if (gltfBinary.HasValue) {
                gLTFastInstance.Load(url, gltfBinary.Value);
            } else {
                gLTFastInstance.Load(url);
            }
        }

        protected virtual void OnLoadComplete(bool success) {
            gLTFastInstance.onLoadComplete -= OnLoadComplete;
            if(success) {
                try {
                    gLTFastInstance.InstantiateGltf(transform);
                } catch (System.Exception e) {
                    loadingErrorMessage = string.Format("{0}", e);
                    success = false;
                }
            } else {
                loadingErrorMessage = gLTFastInstance.LoadingErrorMessage;
            }
            if(onLoadComplete!=null) {
                onLoadComplete(this,success);
            }
        }

        private void OnDestroy()
        {
            if(gLTFastInstance!=null) {
                gLTFastInstance.Destroy();
                gLTFastInstance=null;
            }
        }
    }
}
