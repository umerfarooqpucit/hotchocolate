using System;
using System.Collections.Generic;
using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using static HotChocolate.Data.Projections.WellKnownProjectionFields;

namespace HotChocolate.Data.Projections;

public class ProjectionVisitor<TContext>
    : SelectionVisitor<TContext>
    where TContext : IProjectionVisitorContext
{
    [Obsolete]
    public virtual void Visit(TContext context)
    {
        SelectionSetNode selectionSet =
            context.Context.Selection.SyntaxNode.SelectionSet ?? throw new Exception();
        context.SelectionSetNodes.Push(selectionSet);
        Visit(context.Context.Field, context);
    }

    protected override TContext OnBeforeLeave(ISelection selection, TContext localContext)
    {
        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler)
        {
            return handler.OnBeforeLeave(localContext, selection);
        }

        return localContext;
    }

    protected override TContext OnAfterLeave(
        ISelection selection,
        TContext localContext,
        ISelectionVisitorAction result)
    {
        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler)
        {
            return handler.OnAfterLeave(localContext, selection, result);
        }

        return localContext;
    }

    protected override TContext OnAfterEnter(
        ISelection selection,
        TContext localContext,
        ISelectionVisitorAction result)
    {
        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler)
        {
            return handler.OnAfterEnter(localContext, selection, result);
        }

        return localContext;
    }

    protected override TContext OnBeforeEnter(ISelection selection, TContext context)
    {
        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler)
        {
            return handler.OnBeforeEnter(context, selection);
        }

        return context;
    }

    protected override ISelectionVisitorAction Enter(
        ISelection selection,
        TContext context)
    {
        base.Enter(selection, context);

        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler &&
            handler.TryHandleEnter(
                context,
                selection,
                out ISelectionVisitorAction? handlerResult))
        {
            return handlerResult;
        }

        return SkipAndLeave;
    }

    protected override ISelectionVisitorAction Leave(
        ISelection selection,
        TContext context)
    {
        base.Leave(selection, context);

        if (selection is IProjectionSelection projectionSelection &&
            projectionSelection.Handler is IProjectionFieldHandler<TContext> handler &&
            handler.TryHandleLeave(
                context,
                selection,
                out ISelectionVisitorAction? handlerResult))
        {
            return handlerResult;
        }

        return SkipAndLeave;
    }

    protected override ISelectionVisitorAction Visit(ISelection selection, TContext context)
    {
        if (selection.Field.IsNotProjected())
        {
            return Skip;
        }

        return base.Visit(selection, context);
    }

    protected override ISelectionVisitorAction Visit(IOutputField field, TContext context)
    {
        if (context.SelectionSetNodes.Count > 1 && field.HasProjectionMiddleware())
        {
            return Skip;
        }

        if (field.Type is IPageType and ObjectType pageType &&
            context.SelectionSetNodes.Peek() is { } pagingFieldSelection)
        {
            IReadOnlyList<IFieldSelection> selections =
                context.Context.GetSelections(pageType, pagingFieldSelection, true);

            foreach (IFieldSelection? selection in selections)
            {
                if (selection.ResponseName.Value is CombinedEdgeField)
                {
                    context.SelectionSetNodes.Push(selection.SyntaxNode.SelectionSet);

                    return base.Visit(selection.Field, context);
                }
            }

            return Skip;
        }

        return base.Visit(field, context);
    }
}
