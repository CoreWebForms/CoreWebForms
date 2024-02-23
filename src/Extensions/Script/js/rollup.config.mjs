import preprocess from './rollup-plugin-preprocess.js'
import terser from '@rollup/plugin-terser'

export default [
  {
    input: [
      "MicrosoftAjaxCore.jsa",
      "MicrosoftAjaxWebForms.jsa",
      "MicrosoftAjaxComponentModel.jsa",
      "MicrosoftAjaxGlobalization.jsa",
      "MicrosoftAjaxHistory.jsa",
      "MicrosoftAjaxNetwork.jsa",
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
  },
  {
    input: ["MicrosoftAjax.jsa"],
    output: {
      dir: "dist/",
      format: "es",
    },
    plugins: [
      preprocess(),
      terser(),
    ]
  },
];
