namespace Kula.Core.Ast;

abstract class Type {
    public interface Visitor<R> {
        R VisitLiteral(Literal type);
        R VisitTypeExpr(TypeExpr type);
    }
    public abstract R Accept<R>(Visitor<R> visitor);

    public class Literal : Type {
        public Token name;
        public Literal(Token name) {
            this.name = name;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitLiteral(this);
        }

        public override string ToString() {
            return name.lexeme;
        }
    }

    public class TypeExpr : Type {
        public List<Type> parameters;
        public Type returnType;
        public TypeExpr(List<Type> parameters, Type returnType) {
            this.parameters = parameters;
            this.returnType = returnType;
        }

        public override R Accept<R>(Visitor<R> visitor) {
            return visitor.VisitTypeExpr(this);
        }
        
        public override string ToString() {
            List<string> items = new List<string>();
            foreach (Type type in parameters) {
                items.Add(type?.ToString() ?? "None");
            }
            return $"<{string.Join(',', items)}:{returnType}>";
        }
    }
}