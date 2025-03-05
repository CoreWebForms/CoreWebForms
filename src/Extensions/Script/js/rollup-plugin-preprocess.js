import fs from 'fs'

export default function preprocess() {
  return {
    name: "preprocess",

    async load(id) {
      if (id.endsWith(".jsa") || id.endsWith(".js")) {
        return ProcessFile(id);
      }
    }
  };
}

// Include the current line with a comment if we need to skip it so we can see it for now
function UpdateAccumulator(acc, line, defines, commentLine) {
  if (commentLine) {
    line = "// " + line;
  }

  const newLines = EvaluateDefines(defines) ? [...acc.str, line] : [...acc.str, "// " + line];

  return { str: newLines, defines: defines };
}

function EvaluateDefines(defines) {
  return defines.reduce((acc, cur) => {
    return acc && cur === "true";
  }, true);
}

// The original WebForms code had some C-style preprocessor directives that we need to handle.
// For now, we're just going to strip out debug stuff.
// In the future, we may want to actually evaluate the acc.defines stack to see if the conditions are met
// then we can include it.
function ProcessFile(file) {
  if (file) {
    return fs
      .readFileSync(file, 'utf-8')
      .split("\n")
      .map(str => {
        const include = str.match(/#include\s*\"(.*)\"/);

        if (include) {
          return ProcessFile("./src/" + include[1]);
        }

        return str;
      })
      .join("\n").split("\n")
      .reduce(
        (acc, cur) => {
          const ifdef = cur.match(/^\s*#if\W*(\w*)/)
          const ifelse = cur.match(/^\s*#else/)
          const endif = cur.match(/^\s*#endif/)
          const isdebug = cur.match(/^\s*##DEBUG/)


          if (ifdef) {
            return UpdateAccumulator(acc, cur, [...acc.defines, ifdef[1]]);
          } else if (ifelse) {
            const d = [...acc.defines];
            d.pop();
            return UpdateAccumulator(acc, cur, [...d, "true"], true);
          } else if (endif) {
            const d = [...acc.defines];
            d.pop();
            return UpdateAccumulator(acc, cur, d, true);
          } else if (isdebug) {
            return UpdateAccumulator(acc, cur, acc.defines, true);
          } else {
            return UpdateAccumulator(acc, cur, acc.defines);
          }
        },
        { str: [], defines: [] }
      )
      .str
      .join("\n");
  }
}
