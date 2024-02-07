export default [
  {
    input: [
      "MicrosoftAjaxCore.js",
      "MicrosoftAjaxWebForms.js",
      "MicrosoftAjaxComponentModel.js",
      "MicrosoftAjaxGlobalization.js",
      "MicrosoftAjaxHistory.js",
      "MicrosoftAjaxNetwork.js",
      "MicrosoftAjaxSerialization.js",
    ],
    output: {
      dir: "dist/",
      format: "es",
    },
  },
  {
    input: ["MicrosoftAjax.js"],
    output: {
      dir: "dist/",
      format: "es",
    },
  },
];
