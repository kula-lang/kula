const fs = require("fs");

function lex(source) {
    return source
        .split(';\r')
        .map(i => i.trim().split('::=').map(j => j.trim()))
        .filter(i => i != '')
        .map(item => {
            return {
                name: item[0],
                body: item[1].split(/\s+/)
            }
        });
}

function parse(ls) {
    const len = ls.length;
    let p = 0;

    function _isEnd() { return p >= len; }
    function _isTS(tk) { return /^([A-Z]+|".*")$/.test(tk); }
    function _match(s) { if (_peek() === s) { _advance(); return true; } return false; }
    function _peek() { return ls[p]; }
    function _advance() { return ls[p++]; }

    function eat() {
        if (_match('(')) {
            return eatGroup();
        }
        return _advance();
    }

    function eatGroup() {
        const group = [];
        while (!_match(')')) {
            group.push(eat());
        }
        if (_match('*')) {
            group.unshift('<repeat>');
        }
        else if (_match('?')) {
            group.unshift('<optional>')
        }
        else {
            group.unshift("<choose>");
            return group.filter(i => i != '|');
        }
        return group;
    }

    const root = [];
    while (!_isEnd()) {
        root.push(eat());
    }

    const ret = [[]];
    for (const i of root) {
        if (i == '|') {
            ret.push([]);
        }
        else {
            ret.at(-1).push(i);
        }
    }

    return ret;
}

function generate(asts) {
    
}

function build(source) {
    const tokens = lex(source);
    // for (const token of tokens) {
    //     console.log(token);
    // }
    const asts = tokens.map(i => ({ name: i.name, root: parse(i.body) }));
    for (const ast of asts) {
        console.log(ast.name, ast.root);
    }
}

const ebnfs = ['expr', 'stmt'];
const paths = ebnfs.map(i => `syntax/${i}.ebnf`);
const items = paths.map(i => build(fs.readFileSync(i).toString()));