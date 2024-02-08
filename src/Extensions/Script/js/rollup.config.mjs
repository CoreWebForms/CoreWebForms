import preprocess from './preprocess.js'

export default [
  // {
  //   input: [
  //     "MicrosoftAjaxCore.jsa",
  //     "MicrosoftAjaxWebForms.jsa",
  //     "MicrosoftAjaxComponentModel.jsa",
  //     "MicrosoftAjaxGlobalization.jsa",
  //     "MicrosoftAjaxHistory.jsa",
  //     "MicrosoftAjaxNetwork.jsa",
  //     "MicrosoftAjaxSerialization.jsa",
  //   ],
  //   output: {
  //     dir: "dist/",
  //     format: "es",
  //   },
  //   plugins: [
  //     preprocess()
  //   ]
  // },
  {
    input: ["MicrosoftAjax.jsa"],
    output: {
      dir: "dist/",
      format: "es",
    },
    plugins: [
      preprocess()
    ]
  },
];
