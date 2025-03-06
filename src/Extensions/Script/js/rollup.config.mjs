import preprocess from './rollup-plugin-preprocess.js'
import terser from '@rollup/plugin-terser'

export default [
  {
    input: [
      "MicrosoftAjax.jsa",
      "MicrosoftAjaxCore.jsa",
      "MicrosoftAjaxWebForms.jsa",
      "MicrosoftAjaxComponentModel.jsa",
      "MicrosoftAjaxGlobalization.jsa",
      "MicrosoftAjaxHistory.jsa",
      "MicrosoftAjaxNetwork.jsa",
      "MicrosoftAjaxTimer.jsa",
      "MicrosoftAjaxSerialization.jsa",
    ],
    output: {
      dir: "dist/",
      format: "es",
    },
    plugins: [
      preprocess(),
      terser(),
    ]
  }
];
