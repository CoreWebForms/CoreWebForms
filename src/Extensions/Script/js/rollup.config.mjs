import preprocess from './rollup-plugin-preprocess.js'

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
        ],
        treeshake: false // Disable tree-shaking
    }
];
