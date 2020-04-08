export type Maybe<T> = T | null;
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: string;
  String: string;
  Boolean: boolean;
  Int: number;
  Float: number;
  Date: any;
  URI: any;
};



export type Query = {
   __typename?: 'Query';
  getPost: Post;
  listPosts: ListPostsResponse;
};


export type QueryGetPostArgs = {
  input: GetPostInput;
};


export type QueryListPostsArgs = {
  input: ListPostsInput;
};

export type Post = {
   __typename?: 'Post';
  content: Scalars['String'];
  cover: Scalars['String'];
  createdAt: Scalars['Date'];
  postId: Scalars['ID'];
  title: Scalars['String'];
  updatedAt: Scalars['Date'];
};

export type GetPostInput = {
  postId: Scalars['ID'];
};

export type ListPostsResponse = {
   __typename?: 'ListPostsResponse';
  pageToken: Scalars['String'];
  posts: Array<PostSummary>;
};

export type PostSummary = {
   __typename?: 'PostSummary';
  createdAt: Scalars['Date'];
  postId: Scalars['ID'];
  title: Scalars['String'];
};

export type ListPostsInput = {
  pageSize?: Maybe<Scalars['Int']>;
  pageToken?: Maybe<Scalars['ID']>;
};
